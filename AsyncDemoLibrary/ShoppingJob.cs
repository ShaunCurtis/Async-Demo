using System;/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    public class ShoppingJob : BaseClass, IEnumerable
    {

        public ShoppingJob(Action<string> uiLogger)
        {
            this.UIMessenger = uiLogger;
        }

        public Task ShoppingTask
        {
            get
            {
                if (NeedToShop && ShoppingTaskCompletionSource is null)
                {
                    ShoppingTaskCompletionSource = new TaskCompletionSource<bool>();
                    return ShoppingTaskCompletionSource.Task;
                }
                else if (NeedToShop)
                    return ShoppingTaskCompletionSource.Task;
                else
                    return Task.CompletedTask;
            }
        }

        public bool NeedToShop => List.Count > 0;

        TaskCompletionSource<bool> ShoppingTaskCompletionSource = new TaskCompletionSource<bool>();

        private List<string> List = new List<string>();

        public int TripsToShop { get; private set; } = 0;

        public void Add(string item)
        {
            if (ShoppingTaskCompletionSource is null | ShoppingTaskCompletionSource.Task.IsCompleted)
            {
                this.WriteDirectToUI("  +++> Starting New Shopping List");
                ShoppingTaskCompletionSource = new TaskCompletionSource<bool>();
            }
            List.Add(item);
            this.WriteDirectToUI($"  ===> {item} added to Shopping List");

        }

        public void ShoppingDone()
        {
            List.Clear();
            TripsToShop++;
            this.WriteDirectToUI($"  ===> Cleared Shopping List");
            ShoppingTaskCompletionSource.SetResult(true);
        }

        public IEnumerator GetEnumerator() =>
            new ShoppingJobEnumerator(List);

    }

    public class ShoppingJobEnumerator : IEnumerator
    {
        private List<string> _items = new List<string>();
        private int _cursor;

        object IEnumerator.Current
        {
            get
            {
                if ((_cursor < 0) || (_cursor == _items.Count))
                    throw new InvalidOperationException();
                return _items[_cursor];
            }
        }
        public ShoppingJobEnumerator(List<string> items)
        {
            this._items = items;
            _cursor = -1;
        }
        void IEnumerator.Reset() =>
            _cursor = -1;

        bool IEnumerator.MoveNext()
        {
            if (_cursor < _items.Count)
                _cursor++;

            return (!(_cursor == _items.Count));
        }

    }

}
