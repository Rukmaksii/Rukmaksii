using GameScene.Map;
using GameScene.model;
using GameScene.PlayerControllers.BasePlayer;
using Unity.Netcode;
using UnityEngine;

namespace GameScene.Items
{
    public class Bandage : BaseItem
    {
        public override float Duration { get; } = 2f;
        public override int Price { get; } = 30;

        protected override void Setup()
        {
            Player.EffectiveMovementSpeed = 0;
            Player.Jetpack.JetpackForce = 0;
        }


        
        protected override void OnConsume()
        {
            
        }

        protected override void TearDown()
        {

        }
    }
}