using System.Collections;
using FluentValidation;
using FluentValidation.Results;
using Rishvi.Modules.Core.Content;

namespace Rishvi.Modules.Core.Validators
{
    public abstract class RishviAbstractValidator<T> : AbstractValidator<T>, IValidator<T>, IValidator, IEnumerable<IValidationRule>, IEnumerable
    {
        private bool BlankInstancePreValidate(ValidationContext<T> context, ValidationResult result)
        {
            if (context.InstanceToValidate == null)
            {
                result.Errors.Add(new ValidationFailure("", "Please ensure a model was supplied."));
                return false;
            }

            if (context.InstanceToValidate.GetType() == typeof(List<int>))
            {
                var intList = (IList<int>)context.InstanceToValidate;
                if (!intList.Any())
                {
                    result.Errors.Add(new ValidationFailure("", Messages.SelectAtLeastOneItemFromList));
                    return false;
                }
            }

            return true;
        }

        public override ValidationResult Validate(ValidationContext<T> context)
        {
            var result = new ValidationResult();
            if (!BlankInstancePreValidate(context, result))
            {
                return result;
            }

            result = base.Validate(context);
            if (!result.IsValid)
            {
                return result;
            }

            PostValidate(context, result);

            return result;
        }

        protected virtual void PostValidate(ValidationContext<T> context, ValidationResult result) { }
    }
}