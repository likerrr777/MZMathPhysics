﻿using PoohMathParser;
using System;
using System.Collections.Generic;

namespace _5Kurs_Sybil
{
    public class StaticResources
    {
        public Lazy<Dictionary<EquationsEnum, Settings>> DefaultSettings { get; private set; }
        public Lazy<Dictionary<EquationsEnum, Func<Settings, int, int, double>>> MatrixAInits { get; private set; }
        public Lazy<Dictionary<EquationsEnum, Func<Settings, int, double>>> MatrixBInits { get; private set; }

        public StaticResources()
        {
            DefaultSettings = new Lazy<Dictionary<EquationsEnum, Settings>>(() => GetDefaultSettings());
            MatrixAInits = new Lazy<Dictionary<EquationsEnum, Func<Settings, int, int, double>>>(() => GetMatrixAInits());
            MatrixBInits = new Lazy<Dictionary<EquationsEnum, Func<Settings, int, double>>>(() => GetMatrixBInits());
        }

        private Dictionary<EquationsEnum, Settings> GetDefaultSettings()
        {
            var result = new Dictionary<EquationsEnum, Settings>();

            result.Add(EquationsEnum.Equation_1, new Settings
            {
                AmountOfPartitions = 10,
                IntervalOfFunction = new Tuple<double, double>(-1, 1),
                IntervalOfIntegration = new Tuple<double, double>(-1, 1),
                FunctionDistance = new MathExpression("abs(t-s)"),
                FunctionF = new MathExpression("1"),
                FunctionYakobian = new MathExpression("1"),
                Variables = new List<string> { "t", "s", "a" }
            });

            result.Add(EquationsEnum.Equation_2, new Settings
            {
                AmountOfPartitions = 10,
                IntervalOfFunction = new Tuple<double, double>(-1, 1),
                IntervalOfIntegration = new Tuple<double, double>(-1, 1),
                FunctionDistance = new MathExpression("abs(t-s)"),
                FunctionF = new MathExpression("1"),
                FunctionYakobian = new MathExpression("1"),
                Variables = new List<string> { "t", "s", "a" }
            });

            result.Add(EquationsEnum.Equation_3, new Settings
            {
                AmountOfPartitions = 10,
                IntervalOfFunction = new Tuple<double, double>(-1, 1),
                IntervalOfIntegration = new Tuple<double, double>(-1, 1),
                FunctionDistance = new MathExpression("abs(t-s)"),
                FunctionF = new MathExpression("1"),
                FunctionYakobian = new MathExpression("1"),
                Variables = new List<string> { "t", "s", "a" }
            });

            result.Add(EquationsEnum.Equation_4, new Settings
            {
                AmountOfPartitions = 10,
                IntervalOfFunction = new Tuple<double, double>(0, 2 * Math.PI),
                IntervalOfIntegration = new Tuple<double, double>(0, 2 * Math.PI),
                FunctionDistance = new MathExpression("sqrt((2*a^2)*(1-cos(t-s)))"),
                FunctionF = new MathExpression("1"),
                FunctionYakobian = new MathExpression("a"),
                Radius = 9,
                Variables = new List<string> { "t", "s", "a" }
            });

            return result;
        }
        private Dictionary<EquationsEnum, Func<Settings, int, int, double>> GetMatrixAInits()
        {
            var result = new Dictionary<EquationsEnum, Func<Settings, int, int, double>>();

            result.Add(EquationsEnum.Equation_1, (settings, i, j) =>
            {
                j++;
                return FredholmEquationFirstOrder.Calculate_Aij(settings.PartitionPoints, settings.ColocationPoints, i, j);
            });

            result.Add(EquationsEnum.Equation_2, (settings, i, j) =>
            {
                j++;
                return (settings.PartitionPoints[j] - settings.PartitionPoints[j - 1]) * FredholmEquationFirstOrder.Calculate_Aij(settings.PartitionPoints, settings.ColocationPoints, i, j);
            });

            result.Add(EquationsEnum.Equation_3, (settings, i, j) =>
            {
                j++;
                var h = settings.PartitionPoints[j] - settings.PartitionPoints[j - 1];
                return 1 / (2 * Math.PI) * (GaussMethodForIntegrals.CalculateWithAccuracy(settings.PartitionPoints[j - 1], settings.PartitionPoints[j],
                        new MathExpression($"(t-{FixMinus(settings.PartitionPoints[j - 1])})/{h}*ln(1/({settings.FunctionDistance}))"),
                        0.001, new Var(settings.Variables[0], 0), new Var(settings.Variables[1], settings.ColocationPoints[i])) +
                        GaussMethodForIntegrals.CalculateWithAccuracy(settings.PartitionPoints[j], settings.PartitionPoints[j + 1],
                        new MathExpression($"({FixMinus(settings.PartitionPoints[j + 1])}-t)/{h}*ln(1/({settings.FunctionDistance}))"),
                        0.001, new Var(settings.Variables[0], 0), new Var(settings.Variables[1], settings.ColocationPoints[i])));
            });

            result.Add(EquationsEnum.Equation_4, (settings, i, j) =>
            {
                j++;
                if (i == (j - 1))
                {
                    return settings.Radius.Value / (2 * Math.PI) * (FredholmEquationFirstOrder.Calculate_Aij(settings.PartitionPoints, settings.ColocationPoints, i, j) - Math.Log(settings.Radius.Value) *
                        Math.Abs(settings.IntervalOfIntegration.Item2 - settings.IntervalOfIntegration.Item1) / settings.AmountOfPartitions +
                        GaussMethodForIntegrals.CalculateWithAccuracy(settings.PartitionPoints[j - 1], settings.PartitionPoints[j],
                        new MathExpression($"ln(1/({settings.FunctionDistance}))-ln(1/({settings.Radius.Value}*abs(t-{settings.ColocationPoints[i]})))"),
                        0.001, new Var(settings.Variables[0], 0), new Var(settings.Variables[1], settings.ColocationPoints[i]), new Var(settings.Variables[2], settings.Radius.Value)));
                }
                else
                {
                    return settings.Radius.Value / (2 * Math.PI) * (GaussMethodForIntegrals.CalculateWithAccuracy(settings.PartitionPoints[j - 1], settings.PartitionPoints[j],
                        new MathExpression($"ln(1/({settings.FunctionDistance}))"),
                        0.001, new Var(settings.Variables[0], 0), new Var(settings.Variables[1], settings.ColocationPoints[i]), new Var(settings.Variables[2], settings.Radius.Value)));
                }
            });

            return result;
        }
        private Dictionary<EquationsEnum, Func<Settings, int, double>> GetMatrixBInits()
        {
            var result = new Dictionary<EquationsEnum, Func<Settings, int, double>>();

            result.Add(EquationsEnum.Equation_1, (settings, i) => settings.FunctionF.Calculate(settings.ColocationPoints[i]));
            result.Add(EquationsEnum.Equation_2, (settings, i) =>
            {
                i++;
                return GaussMethodForIntegrals.CalculateWithAccuracy(settings.PartitionPoints[i - 1], settings.PartitionPoints[i], settings.FunctionF,
                        0.001, new Var(settings.Variables[1], 0));
            });
            result.Add(EquationsEnum.Equation_3, (settings, i) => settings.FunctionF.Calculate(settings.PartitionPoints[i]));
            result.Add(EquationsEnum.Equation_4, (settings, i) => settings.FunctionF.Calculate(settings.ColocationPoints[i]));

            return result;
        }

        private string FixMinus(double input)
        {
            string result = input.ToString();
            if (result[0] == '-')
            {
                result = $"(0{result})";
            }
            return result;
        }
    }
}