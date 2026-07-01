namespace Nostrivia
{
    public enum QuestionPhase
    {
        Idle,
        Selected,
        Revealed
    }

    public class GameplayState
    {
        public int CorrectIndex { get; set; } = 1; // Internet Explorer
        public int? Selected { get; private set; }
        public QuestionPhase Phase { get; private set; } = QuestionPhase.Idle;
        public bool CanSubmit => Phase == QuestionPhase.Selected;
        public bool IsSelectedCorrect => Selected.HasValue && Selected.Value == CorrectIndex;

        public void Select(int i)
        {
            Selected = i;
            Phase = QuestionPhase.Selected;
        }

        public bool Submit()
        {
            if (Phase != QuestionPhase.Selected)
            {
                return false;
            }

            Phase = QuestionPhase.Revealed;
            return true;
        }
    }
}
