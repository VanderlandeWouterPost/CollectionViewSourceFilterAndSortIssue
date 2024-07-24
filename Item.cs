using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CollectionViewSourceIssue
{
    public class Item : INotifyPropertyChanged, IFilterable
    {
        public Item(int name)
        {
            Name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Name { get; }

        private bool _isVisibleThroughFilter;
        public bool IsVisibleThroughFilter
        {
            get { return _isVisibleThroughFilter; }
            set { SetField(ref _isVisibleThroughFilter, value); }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}