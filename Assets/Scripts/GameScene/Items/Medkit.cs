using GameScene.Map;
using GameScene.model;
using GameScene.PlayerControllers.BasePlayer;
using Unity.Netcode;
using UnityEngine;

namespace GameScene.Items
{
    public class Medkit : BaseItem
    {
        public override float Duration { get; } = 3f;
        public override int Price { get; } = 30;
        private int HealingAmount = 70;
        private float _baseSpeed;
        private float _baseForce;

        protected override void Setup()
        {
            (_baseSpeed, Player.EffectiveMovementSpeed) = (Player.EffectiveMovementSpeed, Player.EffectiveMovementSpeed/6);
            (_baseForce, Player.JetpackForce) = (Player.JetpackForce, Player.JetpackForce/6);

            Player.healing = true;
        }
        
        
        protected override void OnConsume()
        {
        }

        protected override void TearDown()
        {
            Player.healing = false;

            if (Player.MaxHealth < Player.CurrentHealth + HealingAmount)
                Player.CurrentHealth = Player.MaxHealth;
            else
                Player.CurrentHealth = Player.CurrentHealth + HealingAmount;

            Player.EffectiveMovementSpeed = _baseSpeed;
            Player.JetpackForce = _baseForce;
        }
    }
}