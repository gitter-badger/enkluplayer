﻿using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class PlayModeConfig : MonoBehaviour
    {
        public IUXEventHandler Events;
        public NewItemController NewMenu;
        public ClearAllPropsController ClearAllMenu;
        public QuitController QuitMenu;
        
        public TextAsset TestAssetData;
        public TextAsset TestContentData;
    }
}