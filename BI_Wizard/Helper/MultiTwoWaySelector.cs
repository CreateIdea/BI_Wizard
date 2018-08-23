using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BI_Wizard.Helper
{
    /// <summary>
    /// A sync behaviour for a multiselector.
    /// http://blog.functionalfun.net/2009/02/how-to-databind-to-selecteditems.html
    /// </summary>
    public static class MultiTwoWaySelector
    {
        //TwoWay bindable dependency property to sync multi select items.
        public static readonly DependencyProperty SynchronizedSelectedItems = DependencyProperty.RegisterAttached(
            "SynchronizedSelectedItems",
            typeof(IList),
            typeof(MultiTwoWaySelector),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSynchronizedSelectedItemsChanged));

        //Can be used to specify your won way of converting objects to and from SynchronizedSelectedItems and SelectedItems using ItemsSource
        public static readonly DependencyProperty ItemConverter = DependencyProperty.RegisterAttached(
            "ItemConverter",
            typeof(IListItemConverter),
            typeof(MultiTwoWaySelector),
            new FrameworkPropertyMetadata(null, OnSynchronizedSelectedItemsChanged));

        private static readonly DependencyProperty SynchronizationManagerProperty = DependencyProperty.RegisterAttached(
            "SynchronizationManager",
            typeof(SynchronizationManager),
            typeof(MultiTwoWaySelector),
            new PropertyMetadata(null));

        /// <summary>
        /// Gets the synchronized selected items.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <returns>The list that is acting as the sync list.</returns>
        public static IList GetSynchronizedSelectedItems(DependencyObject dependencyObject)
        {
            return (IList)dependencyObject.GetValue(SynchronizedSelectedItems);
        }


        public static void SetItemConverter(DependencyObject dependencyObject, IListItemConverter value)
        {
            dependencyObject.SetValue(ItemConverter, value);
        }

        public static IListItemConverter GetItemConverter(DependencyObject dependencyObject)
        {
            return (IListItemConverter)dependencyObject.GetValue(ItemConverter);
        }

        /// <summary>
        /// Sets the synchronized selected items.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="value">The value to be set as synchronized items.</param>
        public static void SetSynchronizedSelectedItems(DependencyObject dependencyObject, IList value)
        {
            dependencyObject.SetValue(SynchronizedSelectedItems, value);
        }


        private static SynchronizationManager GetSynchronizationManager(DependencyObject dependencyObject)
        {
            return (SynchronizationManager)dependencyObject.GetValue(SynchronizationManagerProperty);
        }

        private static void SetSynchronizationManager(DependencyObject dependencyObject, SynchronizationManager value)
        {
            dependencyObject.SetValue(SynchronizationManagerProperty, value);
        }

        private static void OnSynchronizedSelectedItemsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                SynchronizationManager synchronizer = GetSynchronizationManager(dependencyObject);
                synchronizer.StopSynchronizing();
                SetSynchronizationManager(dependencyObject, null);
            }

            IList list = e.NewValue as IList;
            Selector selector = dependencyObject as Selector;

            // check that this property is an IList, and that it is being set on a ListBox
            if (list != null && selector != null)
            {
                SynchronizationManager synchronizer = GetSynchronizationManager(dependencyObject);
                IListItemConverter itemConverter = GetItemConverter(dependencyObject);
                if (synchronizer == null)
                {
                    synchronizer = new SynchronizationManager(selector, itemConverter);
                    SetSynchronizationManager(dependencyObject, synchronizer);
                }
                synchronizer.StartSynchronizingList();
            }
        }

        /// <summary>
        /// A synchronization manager.
        /// </summary>
        private class SynchronizationManager
        {
            private readonly Selector _multiSelector;
            private TwoListSynchronizer _synchronizer;
            private readonly IListItemConverter _itemConverter;
            //SelectedValuePath  use it.

            /// <summary>
            /// Initializes a new instance of the <see cref="SynchronizationManager"/> class.
            /// </summary>
            /// <param name="selector">The selector.</param>
            /// <param name="itemConverter"></param>
            internal SynchronizationManager(Selector selector, IListItemConverter itemConverter)
            {
                _multiSelector = selector;
                _itemConverter = itemConverter;
            }

            /// <summary>
            /// Starts synchronizing the list.
            /// </summary>
            public void StartSynchronizingList()
            {
                IList synchronizedSelectedItemsList = GetSynchronizedSelectedItems(_multiSelector);
                IEnumerable itemsSourceList = GetItemsSourceCollection(_multiSelector);
                IList selectedItemsList = GetSelectedItemsCollection(_multiSelector);

                if (synchronizedSelectedItemsList != null)
                {
                    _synchronizer = new TwoListSynchronizer(
                        synchronizedSelectedItemsList,
                        selectedItemsList,
                        _itemConverter,
                        _multiSelector.SelectedValuePath,
                        itemsSourceList);
                    _synchronizer.StartSynchronizing();
                }
            }

            /// <summary>
            /// Stops synchronizing the list.
            /// </summary>
            public void StopSynchronizing()
            {
                _synchronizer.StopSynchronizing();
            }

            public static IList GetSelectedItemsCollection(Selector selector)
            {
                if (selector is MultiSelector)
                {
                    return (selector as MultiSelector).SelectedItems;
                }
                else if (selector is ListBox)
                {
                    return (selector as ListBox).SelectedItems;
                }
                else
                {
                    throw new InvalidOperationException("Target object has no SelectedItems property to bind.");
                }
            }

            public static IEnumerable GetItemsSourceCollection(Selector selector)
            {
                if (selector is MultiSelector)
                {
                    return (selector as MultiSelector).ItemsSource;
                }
                else if (selector is ListBox)
                {
                    return (selector as ListBox).ItemsSource;
                }
                else
                {
                    throw new InvalidOperationException("Target object has no ItemsSource property to bind.");
                }
            }
        }
    }
}

