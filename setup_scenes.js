const fs = require('fs');

const SCENES = [
  { path: 'D:/Test/Fpv_Drone/Assets/Scenes/Warehouse.unity', maxHeight: 150, radius: 400 },
  { path: 'D:/Test/Fpv_Drone/Assets/Scenes/fest.unity', maxHeight: 80, radius: 300 },
];

const SCRIPT_GUID = '72aa7624ca594ecedcad9f901c1992d9';

function injectGameInitializer(scenePath, maxHeight, radius) {
  let content = fs.readFileSync(scenePath, 'utf8');
  const lines = content.split('\n');

  if (content.includes('72aa7624ca594ecedcad9f901c1992d9')) {
    console.log(`  SKIP: already has GameInitializer`);
    return;
  }

  const idRegex = /&(\d+)/g;
  let maxId = 0;
  let m;
  while ((m = idRegex.exec(content)) !== null) {
    const id = parseInt(m[1]);
    if (id > maxId && id < 9007199254740991) maxId = id;
  }

  const nextId = maxId + 1;
  const gameObjId = nextId;
  const transId = nextId + 1;
  const monoId = nextId + 2;

  const gameManagerBlock = [
    `--- !u!1 &${gameObjId}`,
    'GameObject:',
    '  m_ObjectHideFlags: 0',
    '  m_CorrespondingSourceObject: {fileID: 0}',
    '  m_PrefabInstance: {fileID: 0}',
    '  m_PrefabAsset: {fileID: 0}',
    '  serializedVersion: 6',
    '  m_Component:',
    `  - component: {fileID: ${transId}}`,
    `  - component: {fileID: ${monoId}}`,
    '  m_Layer: 0',
    '  m_Name: GameManager',
    '  m_TagString: Untagged',
    '  m_Icon: {fileID: 0}',
    '  m_NavMeshLayer: 0',
    '  m_StaticEditorFlags: 0',
    '  m_IsActive: 1',
    `--- !u!4 &${transId}`,
    'Transform:',
    '  m_ObjectHideFlags: 0',
    '  m_CorrespondingSourceObject: {fileID: 0}',
    '  m_PrefabInstance: {fileID: 0}',
    '  m_PrefabAsset: {fileID: 0}',
    `  m_GameObject: {fileID: ${gameObjId}}`,
    '  serializedVersion: 2',
    '  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}',
    '  m_LocalPosition: {x: 0, y: 0, z: 0}',
    '  m_LocalScale: {x: 1, y: 1, z: 1}',
    '  m_ConstrainProportionsScale: 0',
    '  m_Children: []',
    '  m_Father: {fileID: 0}',
    '  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}',
    `--- !u!114 &${monoId}`,
    'MonoBehaviour:',
    '  m_ObjectHideFlags: 0',
    '  m_CorrespondingSourceObject: {fileID: 0}',
    '  m_PrefabInstance: {fileID: 0}',
    '  m_PrefabAsset: {fileID: 0}',
    `  m_GameObject: {fileID: ${gameObjId}}`,
    '  m_Enabled: 1',
    '  m_EditorHideFlags: 0',
    `  m_Script: {fileID: 11500000, guid: ${SCRIPT_GUID}, type: 3}`,
    '  m_Name: ',
    '  m_EditorClassIdentifier: ',
    `  droneSpawnPosition: {x: 0, y: 2, z: 0}`,
    `  areaMaxHeight: ${maxHeight}`,
    `  areaBoundaryRadius: ${radius}`,
    '  areaCenter: {x: 0, y: 0, z: 0}',
    '',
  ];

  const blockStr = gameManagerBlock.join('\n');

  const sceneRootsIdx = lines.findIndex(l => l.includes('9223372036854775807'));
  if (sceneRootsIdx === -1) {
    console.error('  ERROR: SceneRoots not found!');
    return;
  }

  const rootsEnd = lines.findIndex((l, i) => i > sceneRootsIdx && l.startsWith('---'));
  const rootsSection = lines.slice(sceneRootsIdx, rootsEnd > -1 ? rootsEnd : lines.length);
  const rootLines = [];

  for (const line of rootsSection) {
    rootLines.push(line);
    if (line.trim() === 'm_Roots:') {
      rootLines.push(`  - {fileID: ${transId}}`);
    }
  }

  const beforeRoots = lines.slice(0, sceneRootsIdx);
  const afterRoots = rootsEnd > -1 ? lines.slice(rootsEnd) : [];

  const output = [...beforeRoots, blockStr, rootLines.join('\n'), '', ...afterRoots].join('\n');

  fs.writeFileSync(scenePath, output, 'utf8');
  console.log(`  OK: Injected GameInitializer (maxHeight=${maxHeight}, radius=${radius})`);
}

console.log('Injecting GameInitializer into scenes...');
for (const scene of SCENES) {
  console.log(`Processing: ${scene.path}`);
  injectGameInitializer(scene.path, scene.maxHeight, scene.radius);
}
console.log('=== Done ===');
