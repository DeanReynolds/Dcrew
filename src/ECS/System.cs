using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dcrew.ECS {
    internal class System<T1, T2> where T1 : struct, IComponent where T2 : struct, IComponent {
        internal readonly SparseSet _set = new();

        public System() {
            if (Component<T2>.Count < Component<T1>.Count) {
                foreach (int i in Component<T2>.All)
                    if (Component<T1>.Has(i))
                        _set.Add(i);
            } else
                foreach (int i in Component<T1>.All)
                    if (Component<T2>.Has(i))
                        _set.Add(i);
        }

        public ReadOnlySpan<int> All => _set.All;
    }
}