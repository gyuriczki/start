using System;
using AttaxxPlus.Model;
using AttaxxPlus.ViewModel;

namespace AttaxxPlus.Boosters
{
    /// <summary>
    /// Booster filling all empty fields with the opponents' color (assuming 2 players).
    /// </summary>
    public class SurrenderBooster : BoosterBase
    {
        // EVIP: compact override of getter for Title returning constant.
        public override string Title => "Surrender";

        public SurrenderBooster(GameViewModel gameViewModel) : base(gameViewModel)
        {
            LoadImage(new Uri(@"ms-appx:///Boosters/SurrenderBooster.png"));
            InitializeGame();
        }

        public override bool TryExecute(Field selectedField, Field currentField)
        {
            int winner = this.GameViewModel.CurrentPlayer == 1 ? 2 : 1;
            foreach (var field in this.GameViewModel.Model.Fields)
            {
                if (field.IsEmpty())
                    field.Owner = winner;
            }
            return true;
        }
    }
}
