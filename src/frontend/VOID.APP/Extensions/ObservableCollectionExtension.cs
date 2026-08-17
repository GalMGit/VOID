using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using DynamicData;

namespace VOID.APP.Extensions;

public static class ObservableCollectionExtension
{
    public static async Task UpdateFromAsync<T>(
        this ObservableCollection<T> collection,
        IEnumerable<T> items)
    {
        var dispatcher = Dispatcher.UIThread;

        await dispatcher.InvokeAsync(() =>
        {
            collection.Clear();
            collection.AddRange(items);
        });
    }
}