using System;

namespace PixelVisionRunner.Utils
{
    public delegate void SimplePromise<T>(Action<T> resolve);

    public static class SimplePromise
    {
        public static SimplePromise<TResult> Then<T, TResult>(this SimplePromise<T> self, Func<T, TResult> then)
        {
            return (resolve) =>
            {
                self.Invoke((result) =>
                {
                    resolve(then(result));
                });
            };
        }

        public static void Execute<T>(this SimplePromise<T> self)
        {
            self.Invoke(_ => { });
        }
    }

    public struct Unit
    {
        public static Unit Value { get { return new Unit();  } }
    }
}
