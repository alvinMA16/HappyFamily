/**
 * 主线剧情与关卡内容工厂
 * 小游戏原生版本以“老屋焕新 + 三代亲情”为主线
 */

const ITEMS = {
  bamboo_chair: { label: '竹椅', image: 'images/items/banboo_chair.png' },
  flower_pot: { label: '花盆', image: 'images/items/flower_pot.png' },
  hand_fan: { label: '蒲扇', image: 'images/items/hand_fan.png' },
  wooden_table: { label: '木桌', image: 'images/items/wooden_table.png' },
  wooden_chair: { label: '木椅', image: 'images/items/wooden_chair.png' },

  thermos: { label: '暖壶', image: null },
  enamel_mug: { label: '搪瓷缸', image: null },
  radio: { label: '收音机', image: null },
  basket: { label: '菜篮子', image: null },
  photo_frame: { label: '旧相框', image: null },
  rag: { label: '抹布', image: null },
  teacup: { label: '茶杯', image: null },
  cushion: { label: '靠垫', image: null },
  album: { label: '旧相册', image: null },
  tray: { label: '茶盘', image: null },
  hook: { label: '挂钩', image: null },
  apron: { label: '围裙', image: null },
  soy_sauce: { label: '酱油瓶', image: null },
  cutting_board: { label: '案板', image: null },
  bowl: { label: '碗碟', image: null },
  salt_jar: { label: '盐罐', image: null },
  spoon: { label: '汤勺', image: null },
  chopsticks: { label: '筷笼', image: null },
  plate: { label: '盘子', image: null },
  tablecloth: { label: '桌布', image: null },
  dumpling_tray: { label: '盖帘', image: null },
  pencil: { label: '铅笔', image: null },
  eraser: { label: '橡皮', image: null },
  workbook: { label: '练习本', image: null },
  ruler: { label: '小尺子', image: null },
  desk_lamp: { label: '台灯', image: null },
  textbook_cn: { label: '语文书', image: null },
  textbook_math: { label: '数学书', image: null },
  sketchbook: { label: '画本', image: null },
  pencil_case: { label: '笔袋', image: null },
  bookstand: { label: '书立', image: null },
  clipboard: { label: '作业夹', image: null },
  globe: { label: '地球仪', image: null },
  alarm_clock: { label: '闹钟', image: null },
  color_pen: { label: '彩笔', image: null },
  pillowcase: { label: '枕套', image: null },
  bedsheet: { label: '被单', image: null },
  hanger: { label: '衣架', image: null },
  towel: { label: '毛巾', image: null },
  storage_box: { label: '收纳盒', image: null },
  medicine_box: { label: '药盒', image: null },
  sweater: { label: '毛衣', image: null },
  coat: { label: '外套', image: null },
  scarf: { label: '围巾', image: null },
  cloth_shoes: { label: '布鞋', image: null },
  storage_bag: { label: '收纳袋', image: null },
  watering_can: { label: '喷壶', image: null },
  shovel: { label: '小铲子', image: null },
  clothes_pin: { label: '晒衣夹', image: null },
  window_paper: { label: '窗花', image: null },
  fu_char: { label: '福字', image: null },
  lantern: { label: '灯笼', image: null },
  kettle: { label: '热水壶', image: null },
  slipper: { label: '拖鞋', image: null },
  fruit_plate: { label: '果盘', image: null },
  camera: { label: '相机', image: null },
  candy_box: { label: '糖盒', image: null },
};

function createFamilyMember(id, displayName, relationship, recurringWish) {
  return { id, displayName, relationship, recurringWish };
}

function createRenovationOption(id, displayName, description) {
  return { id, displayName, description };
}

function createRenovationNode(chapterId, id, displayName, requiredStars, storyBeatText, memoryTitle, memoryItem, memoryText, options) {
  return {
    chapterId,
    id,
    displayName,
    requiredStars,
    storyBeatText,
    memoryTitle,
    memoryItem,
    memoryText,
    options,
  };
}

function createLevel(chapter, id, displayName, description, mode, starReward, extra) {
  return {
    id,
    chapterId: chapter.id,
    chapterNumber: chapter.number,
    chapterDisplayName: chapter.displayName,
    familyGoal: chapter.familyGoal,
    spaceTheme: chapter.spaceTheme,
    displayName,
    description,
    mode,
    starReward,
    isStoryLevel: true,
    ...extra,
  };
}

function createChapter(definition) {
  const chapter = {
    ...definition,
    levels: [],
    renovationNode: null,
  };

  chapter.levels = definition.levels.map(level => createLevel(chapter, level.id, level.displayName, level.description, level.mode, level.starReward ?? 1, level.extra));
  chapter.renovationNode = createRenovationNode(
    chapter.id,
    definition.renovationNode.id,
    definition.renovationNode.displayName,
    definition.renovationNode.requiredStars,
    definition.renovationNode.storyBeatText,
    definition.renovationNode.memoryTitle,
    definition.renovationNode.memoryItem,
    definition.renovationNode.memoryText,
    definition.renovationNode.options,
  );

  return chapter;
}

const CAMPAIGN = {
  id: 'campaign_old_house_reunion',
  displayName: '幸福人家',
  summary: '把老屋一点点收拾好，等一家人在春节前回家团圆。',
  longTermGoal: '春节前把老屋重新过顺，热热闹闹吃上一顿团圆饭，再拍一张新的全家福。',
  familyMembers: [
    createFamilyMember('grandma', '陈阿姨', '老屋的操持人', '想把家里收拾得亮堂一点，等孩子们回来。'),
    createFamilyMember('son', '建国', '在外工作的儿子', '想带孩子回来多住几天。'),
    createFamilyMember('daughter_in_law', '小芸', '细心的儿媳', '希望一家人回来住得舒服，也能一起包顿饺子。'),
    createFamilyMember('grandchild', '乐乐', '正在上学的孙辈', '想在奶奶家也有一张能写字画画的小书桌。'),
  ],
  chapters: [
    createChapter({
      id: 'chapter_front_yard',
      number: 1,
      displayName: '第一章：前院起头',
      chapterHook: '建国来电话说，今年春节想带孩子回老屋多住几天，院口得先收拾成个迎人的样子。',
      spaceTheme: '前院和院门',
      familyGoal: '先让家看起来像个欢迎人回来的地方。',
      closingBeat: '院门一修好，老屋先亮了一盏灯，等人回来的心也稳了下来。',
      minigameTags: ['配对', '归位'],
      levels: [
        {
          id: 'story_front_yard_01',
          displayName: '先把院口理顺',
          description: '配对常用旧物，把院口先收拾顺当。',
          mode: 'pairMatch',
          extra: { stepBudget: 10, itemIds: ['thermos', 'enamel_mug', 'flower_pot', 'basket', 'radio', 'bamboo_chair'] },
        },
        {
          id: 'story_front_yard_02',
          displayName: '给花架腾地方',
          description: '把前院杂物归位，让门口显得更精神。',
          mode: 'tidyUp',
          extra: { itemIds: ['flower_pot', 'bamboo_chair', 'hand_fan', 'wooden_chair'], gridWidth: 4, gridHeight: 3, maxLayers: 2 },
        },
      ],
      renovationNode: {
        id: 'node_front_yard_gate',
        displayName: '焕新节点：修院门',
        requiredStars: 2,
        storyBeatText: '门一立正，乐乐第一个说“奶奶家像新的一样”。',
        memoryTitle: '院门边的风',
        memoryItem: '旧门环',
        memoryText: '门口收拾利落了，院子里一下就有了人气。旧木门推开时，像把从前那些平常日子也一并推回来了。',
        options: [
          createRenovationOption('gate_warm_wood', '暖木院门', '更朴素，像熟悉的老院子。'),
          createRenovationOption('gate_olive_green', '墨绿院门', '更清爽，院口显得精神一些。'),
        ],
      },
    }),
    createChapter({
      id: 'chapter_living_room',
      number: 2,
      displayName: '第二章：客厅亮堂',
      chapterHook: '院口整齐了，下一步得把客厅拾掇得亮亮堂堂，让一家人回来有地方坐、有话慢慢聊。',
      spaceTheme: '客厅和窗边',
      familyGoal: '把全家最常待的地方收拾舒坦，让回来的人坐得下、聊得开。',
      closingBeat: '全家福重新挂回墙上，像是先把团圆的样子预演了一遍。',
      minigameTags: ['照片拼图', '家具归类'],
      levels: [
        {
          id: 'story_living_room_01',
          displayName: '擦亮窗台',
          description: '把客厅常用物件配整齐，先把窗边腾出来。',
          mode: 'pairMatch',
          extra: { stepBudget: 10, itemIds: ['rag', 'teacup', 'photo_frame', 'cushion', 'album', 'tray'] },
        },
        {
          id: 'story_living_room_02',
          displayName: '把全家福挂好',
          description: '整理沙发边和照片墙，让客厅更敞亮。',
          mode: 'tidyUp',
          extra: { itemIds: ['photo_frame', 'wooden_chair', 'hand_fan', 'cushion', 'hook'], gridWidth: 4, gridHeight: 4, maxLayers: 2 },
        },
      ],
      renovationNode: {
        id: 'node_living_room_photo_wall',
        displayName: '焕新节点：挂全家福',
        requiredStars: 4,
        storyBeatText: '相框一挂好，建国说回来要给老人和孩子再拍一张新的。',
        memoryTitle: '墙上的合影',
        memoryItem: '老相框',
        memoryText: '那张全家福有些旧了，可每次抬头看见，还是会想起一家人挤在一起笑得前仰后合的样子。',
        options: [
          createRenovationOption('photo_wall_classic', '木框合影墙', '稳当耐看，像旧客厅里的熟悉摆设。'),
          createRenovationOption('photo_wall_bright', '暖色照片墙', '更明亮，让客厅多一点热闹劲。'),
        ],
      },
    }),
    createChapter({
      id: 'chapter_kitchen',
      number: 3,
      displayName: '第三章：厨房有香气',
      chapterHook: '小芸在电话里问，回来那天能不能一起包饺子。厨房只要一整好，团圆饭就有了盼头。',
      spaceTheme: '厨房和餐桌',
      familyGoal: '把灶台和餐桌理顺，让一家人回来时真的能一起做顿饭。',
      closingBeat: '锅碗都归了位，像连饭菜的香气都提前在屋里转开了。',
      minigameTags: ['食材分类', '餐具整理'],
      levels: [
        {
          id: 'story_kitchen_01',
          displayName: '灶台先清出来',
          description: '把厨房台面上的常用家什配好对，做饭才不手忙脚乱。',
          mode: 'pairMatch',
          extra: { stepBudget: 10, itemIds: ['apron', 'soy_sauce', 'cutting_board', 'bowl', 'salt_jar', 'spoon'] },
        },
        {
          id: 'story_kitchen_02',
          displayName: '团圆桌摆出来',
          description: '把餐桌边的物件都整理顺，等家人回来就能热热闹闹开饭。',
          mode: 'tidyUp',
          extra: { itemIds: ['plate', 'bowl', 'chopsticks', 'tablecloth', 'dumpling_tray'], gridWidth: 4, gridHeight: 4, maxLayers: 2 },
        },
      ],
      renovationNode: {
        id: 'node_kitchen_table',
        displayName: '焕新节点：摆好餐桌',
        requiredStars: 6,
        storyBeatText: '小芸看见照片，说回来第一顿饭就想在这张桌子上吃热饺子。',
        memoryTitle: '饭桌边的热气',
        memoryItem: '搪瓷饭盆',
        memoryText: '老饭桌磨出了岁月的光泽。桌边坐满人的时候，最普通的家常菜也会显得格外香。',
        options: [
          createRenovationOption('table_red_cloth', '红格桌布', '更有过节的热闹劲。'),
          createRenovationOption('table_plain_wood', '原木餐桌', '更朴实，像平日里也常有人坐。'),
        ],
      },
    }),
    createChapter({
      id: 'chapter_study_corner',
      number: 4,
      displayName: '第四章：给孩子腾张书桌',
      chapterHook: '乐乐说想在奶奶家也有个写字画画的小地方。不是催成绩，只是想让孩子在家里待得住、学得安稳。',
      spaceTheme: '书桌和学习角',
      familyGoal: '给晚辈准备一个安心学习、安静画画的小角落，写的是陪伴，不是压力。',
      closingBeat: '小书桌摆好了，家里也多了一份向上的劲头。',
      minigameTags: ['文具分类', '课本归位'],
      levels: [
        {
          id: 'story_study_corner_01',
          displayName: '先把桌面清出来',
          description: '把零散文具和旧小物配整齐，给书桌腾出清爽地方。',
          mode: 'pairMatch',
          extra: { stepBudget: 10, itemIds: ['pencil', 'eraser', 'workbook', 'ruler', 'desk_lamp', 'pencil_case'] },
        },
        {
          id: 'story_study_corner_02',
          displayName: '课本分好类',
          description: '把书本和画纸摆顺，让孩子坐下来就能安心写字。',
          mode: 'tidyUp',
          extra: { itemIds: ['textbook_cn', 'textbook_math', 'sketchbook', 'bookstand', 'clipboard'], gridWidth: 4, gridHeight: 4, maxLayers: 3 },
        },
      ],
      renovationNode: {
        id: 'node_study_corner_desk',
        displayName: '焕新节点：安置小书桌',
        requiredStars: 8,
        storyBeatText: '乐乐听说有自己的小书桌，高兴地说要把寒假画的画都带回来给奶奶看。',
        memoryTitle: '窗边的小台灯',
        memoryItem: '旧台灯',
        memoryText: '小时候家里那盏台灯不算亮，却照过很多认真写字的晚上。把它擦亮，像把踏实日子也重新点亮了。',
        options: [
          createRenovationOption('desk_warm_wood', '原木书桌', '温和耐看，坐久了也觉得安心。'),
          createRenovationOption('desk_mint_green', '浅绿书桌', '更轻快，让孩子一眼就喜欢。'),
        ],
      },
    }),
    createChapter({
      id: 'chapter_bedroom_balcony',
      number: 5,
      displayName: '第五章：卧室和阳台',
      chapterHook: '家人回来不是只吃顿饭，还得住得舒服。卧室和阳台收整好，家里才算真正能过日子。',
      spaceTheme: '卧室、衣柜和阳台',
      familyGoal: '把被褥、衣柜和花草都安排妥当，让老屋重新有日常生气。',
      closingBeat: '被子晒出了太阳味，阳台也有了绿意，家终于像是重新住开了。',
      minigameTags: ['衣物分类', '花架拼摆'],
      levels: [
        {
          id: 'story_bedroom_balcony_01',
          displayName: '被褥先晒起来',
          description: '把卧室里零碎的日用品配整齐，先把床边和柜门理开。',
          mode: 'pairMatch',
          extra: { stepBudget: 10, itemIds: ['pillowcase', 'bedsheet', 'hanger', 'towel', 'storage_box', 'medicine_box'] },
        },
        {
          id: 'story_bedroom_balcony_02',
          displayName: '阳台种上新绿',
          description: '把阳台上的旧盆旧架整理顺，好让花草重新长起来。',
          mode: 'tidyUp',
          extra: { itemIds: ['flower_pot', 'watering_can', 'bamboo_chair', 'shovel', 'clothes_pin'], gridWidth: 4, gridHeight: 4, maxLayers: 3 },
        },
      ],
      renovationNode: {
        id: 'node_balcony_flower_rack',
        displayName: '焕新节点：重摆花架',
        requiredStars: 10,
        storyBeatText: '花架收拾好后，邻居送来两盆菜苗，说等孩子回来正好看看新绿。',
        memoryTitle: '阳台上的茉莉',
        memoryItem: '白瓷花盆',
        memoryText: '阳台不大，可只要摆上一盆茉莉、一张小凳子，就总让人觉得日子还有很多细碎的香气。',
        options: [
          createRenovationOption('flower_rack_bamboo', '竹编花架', '更有老屋味道，亲切耐看。'),
          createRenovationOption('flower_rack_white', '白漆花架', '更清亮，让阳台显得轻快。'),
        ],
      },
    }),
    createChapter({
      id: 'chapter_reunion_eve',
      number: 6,
      displayName: '第六章：团圆前夜',
      chapterHook: '离春节只差最后几步，窗花、餐椅、热水壶都得安排妥当。忙完这一阵，就能等门口那阵熟悉的脚步声。',
      spaceTheme: '全屋联动和团圆布置',
      familyGoal: '把全屋做最后整理，让等人回来的期待真正落地。',
      closingBeat: '门外传来热闹的人声，屋里这一年的惦记终于都变成了团圆。',
      minigameTags: ['节庆布置', '座位安排'],
      levels: [
        {
          id: 'story_reunion_eve_01',
          displayName: '年节物件备齐',
          description: '把节庆小物都配整齐，窗边和门边才好布置起来。',
          mode: 'pairMatch',
          extra: { stepBudget: 10, itemIds: ['window_paper', 'fu_char', 'lantern', 'kettle', 'slipper', 'fruit_plate'] },
        },
        {
          id: 'story_reunion_eve_02',
          displayName: '等团圆饭开席',
          description: '把最后几样零碎收拾完，老屋就只等家里人推门进来。',
          mode: 'tidyUp',
          extra: { itemIds: ['wooden_table', 'wooden_chair', 'camera', 'window_paper', 'candy_box'], gridWidth: 4, gridHeight: 4, maxLayers: 3 },
        },
      ],
      renovationNode: {
        id: 'node_reunion_dinner',
        displayName: '焕新节点：布置团圆席',
        requiredStars: 12,
        storyBeatText: '桌椅摆停当的那一刻，陈阿姨终于笑着说，这回大家回来，就真像过年了。',
        memoryTitle: '团圆饭前的一刻',
        memoryItem: '老相机',
        memoryText: '每逢过年，饭还没上齐，大家就先忙着说话。那种热闹不是摆设出来的，却总要先有一张收拾妥帖的桌子接住。',
        options: [
          createRenovationOption('dinner_red_gold', '红火团圆席', '更有年味，适合热热闹闹的团聚。'),
          createRenovationOption('dinner_warm_plain', '暖色家常席', '像平常日子一样亲近，只是人更齐一些。'),
        ],
      },
    }),
  ],
};

const STORY_LEVELS = CAMPAIGN.chapters.flatMap(chapter => chapter.levels);

function isLevelCompleted(saveData, levelId) {
  return !!saveData && typeof saveData.isLevelCompleted === 'function' && saveData.isLevelCompleted(levelId);
}

function getSelection(saveData, nodeId) {
  return saveData && typeof saveData.getRenovationSelection === 'function'
    ? saveData.getRenovationSelection(nodeId)
    : null;
}

export function getItem(itemId) {
  return ITEMS[itemId] || { label: itemId, image: null };
}

export function getCampaign() {
  return CAMPAIGN;
}

export function getStoryLevels() {
  return STORY_LEVELS;
}

export function getLevelById(levelId) {
  return STORY_LEVELS.find(level => level.id === levelId) || null;
}

export function getLevels(mode) {
  return STORY_LEVELS.filter(level => level.mode === mode);
}

export function getLevelItems(levelDef) {
  if (!levelDef || !levelDef.itemIds) {
    return [];
  }

  return levelDef.itemIds.map((itemId, index) => {
    const item = getItem(itemId);
    return {
      id: index,
      itemId,
      label: item.label,
      image: item.image,
    };
  });
}

export function getStoryLevelById(levelId) {
  return getLevelById(levelId);
}

export function getStoryChapterByLevelId(levelId) {
  return CAMPAIGN.chapters.find(chapter => chapter.levels.some(level => level.id === levelId)) || null;
}

export function getChapterById(chapterId) {
  return CAMPAIGN.chapters.find(chapter => chapter.id === chapterId) || null;
}

export function isChapterCompleted(saveData, chapterId) {
  const chapter = getChapterById(chapterId);
  if (!chapter) {
    return false;
  }

  return chapter.levels.every(level => isLevelCompleted(saveData, level.id));
}

export function getNextStoryLevel(levelId) {
  const currentIndex = STORY_LEVELS.findIndex(level => level.id === levelId);
  if (currentIndex < 0 || currentIndex >= STORY_LEVELS.length - 1) {
    return null;
  }

  return STORY_LEVELS[currentIndex + 1];
}

export function getNextStoryLevelForSave(saveData) {
  return STORY_LEVELS.find(level => !isLevelCompleted(saveData, level.id)) || null;
}

export function getCurrentChapterForSave(saveData) {
  const nextLevel = getNextStoryLevelForSave(saveData);
  if (nextLevel) {
    return getChapterById(nextLevel.chapterId);
  }

  return CAMPAIGN.chapters[CAMPAIGN.chapters.length - 1] || null;
}

export function getStoryProgress(saveData) {
  const completedLevels = STORY_LEVELS.filter(level => isLevelCompleted(saveData, level.id)).length;
  return {
    completedLevels,
    totalLevels: STORY_LEVELS.length,
    completedChapters: CAMPAIGN.chapters.filter(chapter => isChapterCompleted(saveData, chapter.id)).length,
    totalChapters: CAMPAIGN.chapters.length,
  };
}

export function getFirstUnlockedPendingRenovationNode(saveData) {
  const totalStars = saveData && typeof saveData.getTotalStars === 'function' ? saveData.getTotalStars() : 0;
  for (const chapter of CAMPAIGN.chapters) {
    const node = chapter.renovationNode;
    if (!node) {
      continue;
    }

    if (totalStars >= node.requiredStars && !getSelection(saveData, node.id)) {
      return node;
    }
  }

  return null;
}

export function getLatestSelectedRenovation(saveData) {
  for (let index = CAMPAIGN.chapters.length - 1; index >= 0; index--) {
    const node = CAMPAIGN.chapters[index].renovationNode;
    const optionId = node ? getSelection(saveData, node.id) : null;
    if (!node || !optionId) {
      continue;
    }

    const option = node.options.find(item => item.id === optionId) || null;
    if (option) {
      return { node, option };
    }
  }

  return null;
}
