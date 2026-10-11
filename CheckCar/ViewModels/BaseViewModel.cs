using CommunityToolkit.Mvvm.ComponentModel;

namespace CheckCar.ViewModels
{
    /// <summary>
    /// Base class for all view models. Provides common state such as
    /// <see cref="IsBusy"/> and <see cref="Title"/>.
    /// </summary>
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        [ObservableProperty]
        private string _title = string.Empty;

        public bool IsNotBusy => !IsBusy;
    }
}
