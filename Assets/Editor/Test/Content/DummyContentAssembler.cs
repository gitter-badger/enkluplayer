﻿using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test
{
    public class DummyContentAssembler : IContentAssembler
    {
        public event Action<GameObject> OnAssemblyComplete;

        public void Setup(ContentData data)
        {
            
        }

        public void UpdateMaterialData(MaterialData material)
        {
            
        }

        public void Teardown()
        {
            
        }
    }
}