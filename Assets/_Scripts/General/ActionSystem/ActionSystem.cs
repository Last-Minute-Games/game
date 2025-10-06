using UnityEngine;

using System;
using System.Collections.Generic;
using System.Collections;


public class ActionSystem : Singleton<ActionSystem>
{
    private List<GameAction> reactions = null;

    public bool IsPeforfming { get; private set; } = false;

    private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();

    private static Dictionary<Type, List<Action<GameAction>>> postSubs = new();

    private static Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();

    public void Perform(GameAction action, System.Action OnPerformFinished = null) {
        if (IsPeforfming) {
            return;
        }
        IsPeforfming = true;
        StartCoroutine(Flow(action,() =>
        {
            IsPeforfming = false;
            OnPerformFinished?.Invoke();

        }));

    }

    public void AddReaction(GameAction gameAction) {
        reactions?.Add(gameAction);
    }

    private IEnumerator Flow(GameAction action, Action OnFlowFinished = null) {
        reactions = action.PreReactions;
        PerformSubscribers(action, preSubs);
        yield return PerformReactions();

        reactions = action.PerformReactions;
        yield return PerformPerformer(action);
        yield return PerformReactions();

        reactions = action.PostReactions;
        PerformSubscribers(action, postSubs);
        yield return PerformReactions();

        OnFlowFinished?.Invoke();
    }

    private IEnumerator PerformPerformer(GameAction action) {
        Type type = action.GetType();
        if (performers.ContainsKey(type)) {
            yield return performers[type](action);
        }
        
    }

    private void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subs) {
        Type type = action.GetType();
        if (subs.ContainsKey(type)) {
            foreach (var sub in subs[type]) {
                sub(action);
            }
        }
    }

    private IEnumerator PerformReactions() {
        foreach (var reaction in reactions) {
            yield return Flow(reaction);
        }
    }

    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction {
        Type type = typeof(T);
        IEnumerator wrappedPerformer(GameAction action) => performer((T)action);
        if (performers.ContainsKey(type)) performers[type] = wrappedPerformer;
        else
        {
            performers.Add(type, wrappedPerformer);
        }
    }

    public static void DetachPerformer<T>() where T : GameAction {
        Type type = typeof(T);
        if(performers.ContainsKey(type)) performers.Remove(type);
        
    }

    public static void SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction { 
        Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        void wrappedReaction(GameAction action) => reaction((T)action);
        if (subs.ContainsKey(typeof(T))) subs[typeof(T)].Add(wrappedReaction);
        else {
            subs.Add(typeof(T), new());
            subs[typeof(T)].Add(wrappedReaction);
        }
    }

    public static void UnsubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        if (subs.ContainsKey(typeof(T)))
        {

            void wrappedReaction(GameAction action) => reaction((T)action);
            subs[typeof(T)].Remove(wrappedReaction);
        }

    }


}
