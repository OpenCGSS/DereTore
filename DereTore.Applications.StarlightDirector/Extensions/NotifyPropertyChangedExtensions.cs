using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DereTore.Applications.StarlightDirector.Extensions {
    public static class NotifyPropertyChangedExtensions {

        // http://www.wpftutorial.net/INotifyPropertyChanged.html
        public static bool ChangeAndNotify<T>(this PropertyChangedEventHandler handler, ref T field, T value, Expression<Func<T>> memberExpression) {
            if (memberExpression == null) {
                throw new ArgumentNullException(nameof(memberExpression));
            }
            var body = memberExpression.Body as MemberExpression;
            if (body == null) {
                throw new ArgumentException("Lambda must return a property.");
            }
            if (EqualityComparer<T>.Default.Equals(field, value)) {
                return false;
            }

            var vmExpression = body.Expression as ConstantExpression;
            if (vmExpression != null) {
                var lambda = Expression.Lambda(vmExpression);
                var vmFunc = lambda.Compile();
                var sender = vmFunc.DynamicInvoke();
                handler?.Invoke(sender, new PropertyChangedEventArgs(body.Member.Name));
            }

            field = value;
            return true;
        }

    }
}
