// MOSIA TE – 54607949
// KOMANE K – 44298919
// BOSELE KV – 46381848
// MABENA T – 50745646
// MLILWANA N – 45756635

namespace BrainWave.APP
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
