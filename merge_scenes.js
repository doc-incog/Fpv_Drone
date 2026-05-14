const fs = require('fs');

function parseBlocks(content) {
  const blocks = [];
  const lines = content.split('\n');
  let start = -1;
  for (let i = 0; i < lines.length; i++) {
    if (lines[i].startsWith('--- !u!')) {
      if (start !== -1) blocks.push({ start, end: i, lines: lines.slice(start, i) });
      start = i;
    }
  }
  if (start !== -1) blocks.push({ start, end: lines.length, lines: lines.slice(start) });
  return blocks;
}

function getBlockId(lines) {
  const m = lines[0].match(/&(\d+)/);
  return m ? parseInt(m[1]) : -1;
}

const cityContent = fs.readFileSync('D:/Test/Fpv_Drone/Assets/Scenes/city.unity', 'utf8');
const sampleContent = fs.readFileSync('D:/Test/Fpv_Drone/Assets/Scenes/SampleScene.unity', 'utf8');
const cityBlocks = parseBlocks(cityContent);
const sampleBlocks = parseBlocks(sampleContent);
const cityLines = cityContent.split('\n');

// ========== Extract from SampleScene ==========
const sampleBlockMap = new Map();
sampleBlocks.forEach(b => sampleBlockMap.set(getBlockId(b.lines), b));

// All IDs to extract from SampleScene
const extractIds = [
  // GameManager objects
  42000001, 42000002, 42000003, 42000004, 42000005, 42000006, 42000007, 42000008, 42000009, 42000010,
  // Drone_Parent PrefabInstance + stripped GO
  914947024, 663462294,
  // Added components on Drone_Parent (DroneController, Rigidbody, MeshCollider)
  819694085, 819694086, 819694088,
  // Added components on Drone_Parent (AudioController, Battery, CrashEffects, FPVController)
  42000011, 42000012, 42000013, 42000014,
  // Camera (1) child of Drone_Parent
  431210979, 431210980, 431210981, 431210982, 431210983,
  // MainCamera stripped from prefab + its script ref
  1791230016, 1791230017, 1791230021,
  // Global Volume (clean - only keep Transform + Volume, remove stray components)
  832575517, 832575518, 832575519,
];

// Assign new IDs
let nextId = 40000001;
const idMap = new Map();
extractIds.forEach(id => idMap.set(id, nextId++));

function replaceIds(text) {
  let result = text;
  for (const [oldId, newId] of idMap) {
    // Match {fileID: OLDID} (but NOT {fileID: OLDID, guid:...} since those are prefab refs)
    const regex = new RegExp(`\\{fileID: ${oldId}\\}(?!,)`, 'g');
    result = result.replace(regex, `{fileID: ${newId}}`);
    // Match &OLDID in headers
    result = result.replace(new RegExp(`&${oldId}`, 'g'), `&${newId}`);
  }
  return result;
}

// Build the clean Global Volume YAML manually (since it's custom)
const cleanGV = [
  { type: 1, id: idMap.get(832575517), content: [
    'GameObject:',
    '  m_ObjectHideFlags: 0',
    '  m_CorrespondingSourceObject: {fileID: 0}',
    '  m_PrefabInstance: {fileID: 0}',
    '  m_PrefabAsset: {fileID: 0}',
    '  serializedVersion: 6',
    '  m_Component:',
    `  - component: {fileID: ${idMap.get(832575519)}}`,
    `  - component: {fileID: ${idMap.get(832575518)}}`,
    '  m_Layer: 0',
    '  m_Name: Global Volume',
    '  m_TagString: Untagged',
    '  m_Icon: {fileID: 0}',
    '  m_NavMeshLayer: 0',
    '  m_StaticEditorFlags: 0',
    '  m_IsActive: 1',
  ]},
  { type: 114, id: idMap.get(832575518), content: [
    'MonoBehaviour:',
    '  m_ObjectHideFlags: 0',
    '  m_CorrespondingSourceObject: {fileID: 0}',
    '  m_PrefabInstance: {fileID: 0}',
    '  m_PrefabAsset: {fileID: 0}',
    `  m_GameObject: {fileID: ${idMap.get(832575517)}}`,
    '  m_Enabled: 1',
    '  m_EditorHideFlags: 0',
    '  m_Script: {fileID: 11500000, guid: 172515602e62fb746b5d573b38a5fe58, type: 3}',
    '  m_Name: ',
    '  m_EditorClassIdentifier: ',
    '  m_IsGlobal: 1',
    '  priority: 0',
    '  blendDistance: 0',
    '  weight: 1',
    '  sharedProfile: {fileID: 11400000, guid: 10fc4df2da32a41aaa32d77bc913491c, type: 2}',
  ]},
  { type: 4, id: idMap.get(832575519), content: [
    'Transform:',
    '  m_ObjectHideFlags: 0',
    '  m_CorrespondingSourceObject: {fileID: 0}',
    '  m_PrefabInstance: {fileID: 0}',
    '  m_PrefabAsset: {fileID: 0}',
    `  m_GameObject: {fileID: ${idMap.get(832575517)}}`,
    '  serializedVersion: 2',
    '  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}',
    '  m_LocalPosition: {x: 0, y: 0, z: 0}',
    '  m_LocalScale: {x: 1, y: 1, z: 1}',
    '  m_ConstrainProportionsScale: 0',
    '  m_Children: []',
    '  m_Father: {fileID: 0}',
    '  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}',
  ]},
];

// Collect all extracted block content
let newContentLines = [];

// Add clean Global Volume
cleanGV.forEach(block => {
  newContentLines.push(`--- !u!${block.type} &${block.id}`);
  block.content.forEach(line => newContentLines.push(line));
  newContentLines.push('');
});

// Add extracted blocks from SampleScene (skipping Global Volume originals since we have clean ones)
extractIds.forEach(id => {
  if (id >= 832575517 && id <= 832575519) return; // Skip original Global Volume blocks, use clean ones
  const block = sampleBlockMap.get(id);
  if (block) {
    const remapped = replaceIds(block.lines.join('\n'));
    newContentLines.push(remapped);
  } else {
    console.error(`WARNING: Block ${id} not found in SampleScene!`);
  }
});

// ========== Remove Main Camera + old SceneRoots from city ==========
const cameraIds = new Set([1650532112, 1650532113, 1650532114, 1650532115]);
const blocksToRemove = new Set();
cityBlocks.forEach(b => {
  if (cameraIds.has(getBlockId(b.lines))) blocksToRemove.add(b.start);
});

// Find old SceneRoots block
let sceneRootsBlock = null;
cityBlocks.forEach(b => {
  if (b.lines[0].includes('9223372036854775807')) sceneRootsBlock = b;
});

// Get old roots
const oldRoots = [];
if (sceneRootsBlock) {
  sceneRootsBlock.lines.forEach(line => {
    const m = line.match(/- {fileID: (\d+)}/);
    if (m) oldRoots.push(parseInt(m[1]));
  });
  blocksToRemove.add(sceneRootsBlock.start);
}

// Build output, skipping removed blocks
const outputLines = [];
for (let i = 0; i < cityLines.length; i++) {
  let skip = false;
  for (const start of blocksToRemove) {
    const block = cityBlocks.find(b => b.start === start);
    if (block && i >= block.start && i < block.end) {
      skip = true;
      break;
    }
  }
  if (!skip) outputLines.push(cityLines[i]);
}

// Add new content
outputLines.push('');
outputLines.push(...newContentLines);

// Build new SceneRoots
const newRoots = oldRoots.filter(id => !cameraIds.has(id));
newRoots.push(idMap.get(42000002));   // GameManager Transform
newRoots.push(idMap.get(914947024));  // Drone_Parent PrefabInstance
newRoots.push(idMap.get(832575519)); // Global Volume Transform

// Add SceneRoots block
const rootsLines = [
  `--- !u!1660057539 &9223372036854775807`,
  'SceneRoots:',
  '  m_ObjectHideFlags: 0',
  '  m_Roots:',
];
newRoots.forEach(id => rootsLines.push(`  - {fileID: ${id}}`));
outputLines.push('');
outputLines.push(...rootsLines);

fs.writeFileSync('D:/Test/Fpv_Drone/Assets/Scenes/city.unity', outputLines.join('\n') + '\n', 'utf8');

console.log('=== Merge Complete ===');
console.log(`Removed ${blocksToRemove.size} blocks (camera + old SceneRoots)`);
console.log(`Added GameManager + Drone + Global Volume blocks`);
console.log(`New SceneRoots: ${newRoots.join(', ')}`);
