using System;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.WpfExtensions
{
    public sealed class DelegatedCommand : ICommand
    {
        private readonly Predicate _canExecuteMethod;
        private readonly Action _executeMethod;
        private readonly object _param;
        private readonly Action<object> _action1;
        private readonly Predicate<object> _canExecuteMethod1;

        public string Name { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (this._canExecuteMethod == null && this._canExecuteMethod1 == null)
                    return;
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (this._canExecuteMethod == null && this._canExecuteMethod1 == null)
                    return;
                CommandManager.RequerySuggested -= value;
            }
        }

        public DelegatedCommand(Action executeMethod)
            : this(executeMethod, (Predicate)null)
        {
        }

        public DelegatedCommand(Action executeMethod, Predicate canExecuteMethod)
        {
            if (executeMethod == null)
                throw new ArgumentNullException("executeMethod", "Execute delegate cannot be null");
            this._executeMethod = executeMethod;
            this._canExecuteMethod = canExecuteMethod;
        }

        public DelegatedCommand(Action<object> executeMethod, object param)
            : this(executeMethod, (Predicate<object>)null, param)
        {
        }

        public DelegatedCommand(Action<object> executeMethod, Predicate<object> canExecuteMethod, object param)
        {
            this._param = param;
            this._action1 = executeMethod;
            this._canExecuteMethod1 = canExecuteMethod;
        }

        public bool CanExecute(object parameter)
        {
            if (this._canExecuteMethod == null && this._canExecuteMethod1 == null)
                return true;
            if (this._canExecuteMethod1 != null)
                return this._canExecuteMethod1(this._param);
            if (this._canExecuteMethod != null)
                return this._canExecuteMethod();
            else
                return false;
        }

        public void Execute(object parameter)
        {
            if (this._executeMethod != null)
                this._executeMethod();
            if (this._action1 == null)
                return;
            this._action1(this._param);
        }
    }

    public sealed class DelegatedCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecuteMethod;
        private readonly Action<T> _executeMethod;

        public string Name { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (this._canExecuteMethod == null)
                    return;
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (this._canExecuteMethod == null)
                    return;
                CommandManager.RequerySuggested -= value;
            }
        }

        public DelegatedCommand(Action<T> executeMethod)
            : this(executeMethod, (Predicate<T>)null)
        {
        }

        public DelegatedCommand(Action<T> executeMethod, Predicate<T> canExecuteMethod)
        {
            if (executeMethod == null)
                throw new ArgumentNullException("executeMethod", "Execute delegate cannot be null");
            this._executeMethod = executeMethod;
            this._canExecuteMethod = canExecuteMethod;
        }

        public bool CanExecute(object parameter)
        {
            if (this._canExecuteMethod == null)
                return true;
            else
                return this._canExecuteMethod((T)parameter);
        }

        public void Execute(object parameter)
        {
            if (this._executeMethod == null)
                return;
            this._executeMethod((T)parameter);
        }
    }
}
