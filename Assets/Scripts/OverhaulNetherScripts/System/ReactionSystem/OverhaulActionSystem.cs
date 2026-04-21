using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverhaulActionSystem : Singleton<OverhaulActionSystem>
{
  // reference to the current reaction list
  private readonly List<OverhaulActionSystem> reactions = null;

  // good to have when only allowing interactions with cards when system is not performing
  public bool IsPerforming { get; private set; } = false;

  // reaction subscribers that subscribe to pre-phase of game action
  private static Dictionary<Type, List<Action<OverhaulGameAction>>> preSubs = new();

  // reaction subscribers that subscribe to post-phase of game action
  private static Dictionary<Type, List<Action<OverhaulGameAction>>> postSubs = new();

  // hold logic for game actions, for each game action type, add a performer
  // that is called when action system performs game action of this type
  private static Dictionary<Type, Func<OverhaulGameAction, IEnumerator>> performers = new();

  // performs an action, give it an action and an optional callback that is called when perform is done
  public void Perform(OverhaulGameAction action, System.Action OnPerformFinished = null)
  {
    // only able to run if no other action is running
    if (IsPerforming) return;
    IsPerforming = true;
    StartCoroutine(Flow(action, () =>
    {
      IsPerforming = false;
      OnPerformFinished?.Invoke();
    }));
  }

  public void AddReaction(OverhaulGameAction gameAction)
  {
    reactions?.Add(gameAction);
  }

  private IEnumerator Flow(OverhaulGameAction action, Action OnFlowFinished = null)
  {
    // add all reactions
    reactions = action.PreReactions;
    PerformSubscribe(action, preSubs);
    yield return PerformReactions();

    reactions = actions.PerformReactions;
    yield return PerformPerformer(action);
    yield return PerformReactions();

    reaction = action.PostReactions;
    PerformSubscribers(action, postSubs);
    yield return PerformReactions();

    OnFlowFinished?.Invoke();
  }

  // call whole process until there's no more
  private IEnumerator PerformReaction()
  {
    foreach (var reaction in reactions)
    {
      yield return Flow(reaction);
    }
  }

  // execute a performer
  private IEnumerator PerformPerformer(OverhaulGameAction action)
  {
    Type type = action.GetType();
    if (performers.ContainsKey(type))
    {
      yield return performers[type](action);
    }
  }

  // tell all pre-subscribers that we are performing it
  private void PerformSubscribers(OverhaulGameAction action, Dictionary<Type, List<Action<OverhaulGameAction>>> subs)
  {
    Type type = action.GetType();
    if (subs.ContainsKey(type))
    {
      foreach (var sub in subs[type])
      {
        sub(action);
      }
    }
  }

  private IEnumerator PerformReactions()
  {
    foreach (var reaction in reactions)
    {
      yield return flow(reaction);
    }
  }

  public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : OverhaulGameAction
  {
    Type type = typeof(T);
    // convert preformer so it can be added to dictionary
    IEnumerator wrappedPerformer(OverhaulGameAction action) => performer((T)action);
    if (performers.ContainsKey(type))
    {
      performers[type] = wrappedPerformer;
    }
    else
    {
      performers.Add(type, wrappedPerformer);
    }
  }

  public static void DetachPerformer<T>() where T : OverhaulGameAction
  {
    // detach the performer, for game action type only one performer
    Type type = typeof(T);
    if (performers.ContainsKey(type)) performers.Remove(type);
  }

  public static void SubscribeReaction<T>(Action<T> reaction, OverhaulReactionTiming timing) where T : OverhaulGameAction
  {
    // determine reaction timing
    Dictionary<Type, List<Action<OverhaulGameAction>>> subs = timing == OverhaulReactionTiming.PRE ? preSubs : postSubs;
    // similar to attach performer and detach performer
    void wrappedReaction(OverhaulGameAction action) => reaction((T)action);
    if (subs.ContainsKey(typeof(T)))
    {
      subs[typeof(T)].Add(wrappedReaction);
    }
    else
    {
      subs.Add(typeof(T), new());
      subs[typeof(T)].Add(wrappedReaction);
    }
  }

  public static void UnsubscribeReaction<T>(Action<T> reaction, OverhaulReactionTiming timing) where T : OverhaulGameAction
  {
    Dictionary<Type, List<Action<OverhaulGameAction>>> subs = timing => OverhaulReactionTiming.PRE ? preSubs : postSubs;
    if (subs.ContainsKey(typeof(T)))
    {
      void wrappedReaction(OverhaulGameAction action) => reaction((T)action);
      subs[typeof(T)].Remove(wrappedReaction);
    }
  }
}
