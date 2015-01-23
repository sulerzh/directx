using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Microsoft.Data.Visualization.WpfExtensions
{
    public abstract class ValidationElement : CompositePropertyChangeNotificationBase, IDataErrorInfo, IValidationElement
    {
        private Dictionary<PropertyInfo, List<IPropertyValidationAttribute>> _validationEntries = new Dictionary<PropertyInfo, List<IPropertyValidationAttribute>>();
        private bool _isValidationEnabled = true;

        public bool IsValidationEnabled
        {
            get
            {
                return this._isValidationEnabled;
            }
            set
            {
                this._isValidationEnabled = value;
            }
        }

        public string Error { get; set; }

        public string this[string propertyName]
        {
            get
            {
                return this.ValidateProperty(propertyName);
            }
        }

        protected Dictionary<PropertyInfo, List<IPropertyValidationAttribute>> ValidationEntries
        {
            get
            {
                return this._validationEntries;
            }
        }

        protected ValidationElement()
        {
            this.InitializeValidationEntries();
        }

        private void AddAttributeRulesForProperty(PropertyInfo propInfo)
        {
            foreach (object obj in propInfo.GetCustomAttributes(typeof(IPropertyValidationAttribute), true))
            {
                IPropertyValidationAttribute rule = obj as IPropertyValidationAttribute;
                if (rule != null)
                {
                    rule.ValidateResourceDescription();
                    this.AddPropertyValidationRule(propInfo, rule);
                }
            }
        }

        private void AddChildValidationElementRule(PropertyInfo propInfo)
        {
            bool flag = false;
            foreach (object obj in propInfo.GetCustomAttributes(typeof(IPropertyValidationAttribute), true))
            {
                IPropertyValidationAttribute rule = (IPropertyValidationAttribute)(obj as ChildValidationElementAttribute);
                if (rule != null)
                {
                    this.AddPropertyValidationRule(propInfo, rule);
                    flag = true;
                    break;
                }
                else if ((IPropertyValidationAttribute)(obj as ExcludeChildValidationElementAttribute) != null)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
                return;
            this.AddPropertyValidationRule(propInfo, (IPropertyValidationAttribute)new ChildValidationElementAttribute());
        }

        private void AddPropertyValidationRule(PropertyInfo propInfo, IPropertyValidationAttribute rule)
        {
            if (this._validationEntries.ContainsKey(propInfo))
                this._validationEntries[propInfo].Add(rule);
            else
                this._validationEntries.Add(propInfo, new List<IPropertyValidationAttribute>()
        {
          rule
        });
        }

        private void InitializeValidationEntries()
        {
            foreach (PropertyInfo propInfo in this.GetType().GetProperties())
            {
                if (TypeUtility.PropertyImplementsInterface(propInfo, typeof(IValidationElement)))
                    this.AddChildValidationElementRule(propInfo);
                else
                    this.AddAttributeRulesForProperty(propInfo);
            }
        }

        public virtual bool IsContentValid()
        {
            if (!this.IsValidationEnabled)
                return true;
            foreach (PropertyInfo propInfo in this._validationEntries.Keys)
            {
                if (!string.IsNullOrEmpty(this.ValidateProperty(propInfo)))
                    return false;
            }
            return true;
        }

        private string ValidateProperty(PropertyInfo propInfo)
        {
            if ((PropertyInfo)null == propInfo)
                throw new ArgumentException("propInfo cannot be null", "propInfo");
            string str = IDataErrorInfoConstants.ValidationResultSuccess;
            if (!this._validationEntries.ContainsKey(propInfo))
                return str;
            foreach (IPropertyValidationAttribute validationAttribute in this._validationEntries[propInfo])
            {
                if (!validationAttribute.IsValid(propInfo.GetValue((object)this, (object[])null), (object)this))
                {
                    str = validationAttribute.ErrorMessage;
                    break;
                }
            }
            return str;
        }

        private string ValidateProperty(string propertyName)
        {
            return this.ValidateProperty(this.GetType().GetProperty(propertyName));
        }
    }
}
