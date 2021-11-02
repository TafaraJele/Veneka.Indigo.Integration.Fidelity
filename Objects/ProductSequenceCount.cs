using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Veneka.Indigo.Integration.Fidelity.Objects
{
    sealed class ProductSequenceCount
    {
        public ProductSequenceCount(int productId, int startSequence)
        {
            ProductId = productId;
            StartSequence = startSequence;
        }

        public void Increment()
        {
            Count++;
        }

        public int ProductId { get; private set; }
        public int StartSequence { get; private set; }
        public int Count { get; private set; }
        public int CurrentSequence
        {
            get
            {
                return StartSequence + Count;
            }
        }

    }
}
