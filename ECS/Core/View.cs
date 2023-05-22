using System;
using Core.Game.EntitasInfrastructure;
using DG.Tweening;
using Entitas;
using Entitas.Unity;
using UnityEngine;

public class View : MonoBehaviour, IGameEntityView
{
    public GameEntity GameEntity;

    public virtual void Link(GameEntity entity)
    {
        GameEntity = entity;
        gameObject.Link(entity);
        entity.AddTransform(transform);
        entity.AddHashCode(GetHashCode());
       // entity.AddDestroyedListener(this);
        entity.OnDestroyEntity += EntityOnOnDestroyEntity;
    }

    private void EntityOnOnDestroyEntity(IEntity entity)
    {
        entity.OnDestroyEntity -= EntityOnOnDestroyEntity;
        if (gameObject.GetEntityLink() != null)
        {
            gameObject.Unlink();
        }
        
        GameEntity = null;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (GameEntity != null)
        {
            Debug.Log(gameObject.GetEntityLink());
            GameEntity = null;
            gameObject.Unlink();
        }
         
    }
}
