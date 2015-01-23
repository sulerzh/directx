using System;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public class ConcurrentValue<T> : ConcurrentValueBase<T>
  {
    public T ValueByReading
    {
      get
      {
        return this.SelectByReading<T>((Func<T, T>) (k => k));
      }
    }

    public ConcurrentValue(T _start)
      : base(_start)
    {
    }

    public void UseSafely(Action<T> func)
    {
      try
      {
        T obj = this.BeginUse();
        func(obj);
      }
      finally
      {
        this.EndUse();
      }
    }

    public X SelectSafely<X>(Func<T, X> func)
    {
      X x = default (X);
      try
      {
        T obj = this.BeginUse();
        return func(obj);
      }
      finally
      {
        this.EndUse();
      }
    }

    public override string ToString()
    {
      return this.SelectByReading<string>((Func<T, string>) (c => c.ToString()));
    }

    public T ReplaceWithSafely(T withVal)
    {
      T obj = default (T);
      try
      {
        return this.BeginUse();
      }
      finally
      {
        this.EndUseAndUpdate(withVal);
      }
    }

    public T ModifySafely(Func<T, T> func)
    {
      bool flag = false;
      T updatedValue = default (T);
      try
      {
        T obj = this.BeginUse();
        updatedValue = func(obj);
        this.EndUseAndUpdate(updatedValue);
        flag = true;
      }
      finally
      {
        if (!flag)
          this.EndUse();
      }
      return updatedValue;
    }

    public ConcurrentValue<T>.LockedValue Lock()
    {
      return new ConcurrentValue<T>.LockedValue(this);
    }

    public class LockedValue : IDisposable
    {
      private ConcurrentValue<T> mSA;
      private T mValue;

      public T Value
      {
        get
        {
          return this.mValue;
        }
      }

      public LockedValue(ConcurrentValue<T> sa)
      {
        this.mSA = sa;
        this.mValue = this.mSA.BeginUse();
      }

      public void Dispose()
      {
        this.mValue = default (T);
        this.mSA.EndUse();
      }
    }
  }
}
