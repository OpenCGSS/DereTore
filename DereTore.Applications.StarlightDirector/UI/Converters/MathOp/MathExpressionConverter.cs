using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace DereTore.Applications.StarlightDirector.UI.Converters.MathOp {
    public sealed class MathExpressionConverter : List<MathConverterBase>, IMultiValueConverter {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        // TODO: Implement priorities.
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var resultStack = new Stack<double>();
            var valueIndex = 0;
            var converters = this;
            var hasValue = new List<bool> { false };
            var level = 0;

            for (var converterIndex = 0; converterIndex < Count; ++converterIndex) {
                var converter = converters[converterIndex];
                var valueExistsInCurrentLevel = hasValue[level];
                if (converter is OpSymbolConverterBase) {
                    if (converter is LeftParenConverter) {
                        ++level;
                        if (hasValue.Count <= level) {
                            hasValue.Add(false);
                        } else {
                            hasValue[level] = false;
                        }
                    } else if (converter is RightParenConverter) {
                        var b = hasValue[level];
                        --level;
                        if (b) {
                            hasValue[level] = true;
                        }
                    } else {
                        throw new ArgumentException("Unknown math symbol converter type.");
                    }
                } else if (converter is UnaryOpConverterBase) {
                    var unaryConverter = (UnaryOpConverterBase)converter;
                    if (valueExistsInCurrentLevel) {
                        // Consume 0 value
                        var current = (double)unaryConverter.Convert(resultStack.Pop(), null, null, culture);
                        resultStack.Push(current);
                    } else {
                        // Consume 1 value
                        var current = (double)unaryConverter.Convert(values[valueIndex], null, null, culture);
                        ++valueIndex;
                        resultStack.Push(current);
                    }
                    hasValue[level] = true;
                } else if (converter is BinaryOpConverterBase) {
                    var binaryConverter = (BinaryOpConverterBase)converter;
                    var binaryValues = new object[2];
                    if (valueExistsInCurrentLevel) {
                        // Prefer the ones in value array
                        if (valueIndex < values.Length) {
                            // Consume 1 value
                            binaryValues[0] = resultStack.Pop();
                            binaryValues[1] = values[valueIndex];
                            ++valueIndex;
                        } else {
                            // Consume 0 value
                            // Remember to reverse the order.
                            binaryValues[1] = resultStack.Pop();
                            binaryValues[0] = resultStack.Pop();
                        }
                        var current = (double)binaryConverter.Convert(binaryValues, null, null, culture);
                        resultStack.Push(current);
                    } else {
                        // Consume 2 values
                        binaryValues[0] = values[valueIndex];
                        ++valueIndex;
                        binaryValues[1] = values[valueIndex];
                        ++valueIndex;
                        var current = (double)binaryConverter.Convert(binaryValues, null, null, culture);
                        resultStack.Push(current);
                    }
                    hasValue[level] = true;
                } else {
                    throw new ArgumentException("Unknown math converter type.");
                }
            }

            return resultStack.Pop();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

    }
}
