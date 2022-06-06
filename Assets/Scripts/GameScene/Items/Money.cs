using System.Collections;
using System.Collections.Generic;
using GameScene.Items;
using UnityEngine;

public class Money : BaseItem
{
    public override float Duration { get; } = 0f;
    public override int Price { get; } = 200;

    protected override void Setup()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnConsume()
    {
        throw new System.NotImplementedException();
    }

    protected override void TearDown()
    {
        throw new System.NotImplementedException();
    }
}
