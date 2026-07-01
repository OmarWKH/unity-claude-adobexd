using NUnit.Framework; using Nostrivia;
public class GameplayStateMachineTests {
  [Test] public void Starts_idle_cannot_submit(){ var s=new GameplayState(); Assert.AreEqual(QuestionPhase.Idle, s.Phase); Assert.IsFalse(s.CanSubmit); }
  [Test] public void Select_enables_submit(){ var s=new GameplayState(); s.Select(0); Assert.AreEqual(QuestionPhase.Selected,s.Phase); Assert.IsTrue(s.CanSubmit); }
  [Test] public void Submit_without_selection_fails(){ var s=new GameplayState(); Assert.IsFalse(s.Submit()); Assert.AreEqual(QuestionPhase.Idle,s.Phase); }
  [Test] public void Submit_reveals(){ var s=new GameplayState(); s.Select(1); Assert.IsTrue(s.Submit()); Assert.AreEqual(QuestionPhase.Revealed,s.Phase); }
  [Test] public void Correct_index_is_IE(){ var s=new GameplayState(); s.Select(1); s.Submit(); Assert.IsTrue(s.IsSelectedCorrect); }
  [Test] public void Wrong_selection(){ var s=new GameplayState(); s.Select(0); s.Submit(); Assert.IsFalse(s.IsSelectedCorrect); }
}
