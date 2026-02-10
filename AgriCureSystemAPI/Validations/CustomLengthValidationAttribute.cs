using System.ComponentModel.DataAnnotations;

namespace AgriCureSystemAPI.Validations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CustomLengthValidationAttribute : ValidationAttribute
    {
        private readonly int _length;

        public CustomLengthValidationAttribute(int length)
        {
            _length = length;
        }

        public override bool IsValid(object? value)
        {
            if(value is string v)
            {
                if(v.Length > _length)
                {
                    return true;
                }
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return base.FormatErrorMessage(name);
        }
    }
}
