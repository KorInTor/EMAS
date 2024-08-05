using CommunityToolkit.Mvvm.ComponentModel;

namespace Model
{
    public class SelectableObject<T> : ObservableObject
    {
        public SelectableObject(T newobject)
        {
            Object = newobject;
        }

        private T _object;

        private bool _isSelected;

        public T Object
        {
            get => _object;
            set => SetProperty(ref _object, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
