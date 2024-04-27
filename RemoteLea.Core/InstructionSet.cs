using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RemoteLea.Core;

/// <summary>
/// A set of instructions that can be executed in order
/// </summary>
public class InstructionSet
{
    private readonly List<Instruction> _instructions;
    private readonly Dictionary<string, int> _labelIndices = new(StringComparer.OrdinalIgnoreCase);

    public int Count => _instructions.Count;

    public InstructionSet(List<Instruction> instructions)
    {
        _instructions = instructions;
        for (var x = 0; x < instructions.Count; x++)
        {
            var instruction = instructions[x];
            if (instruction.Label != null)
            {
                if (!_labelIndices.TryAdd(instruction.Label.Trim(), x))
                {
                    var message = $"Multiple instructions contain the label '{instruction.Label}'. Labels must be unique";
                    throw new InvalidOperationException(message);
                }
            }
        }
    }

    /// <summary>
    /// Creates a new enumerator for the instruction set.
    /// </summary>
    /// <returns></returns>
    public InstructionSetEnumerator GetEnumerator()
    {
        return new InstructionSetEnumerator(this);
    }

    /// <summary>
    /// Enumerates each instruction in the instruction set.
    /// </summary>
    public struct InstructionSetEnumerator : IEnumerator<Instruction?>
    {
        private readonly InstructionSet _instructionSet;
        private int _currentIndex;
        private bool _hasStarted = false;

        public Instruction? Current { get; set; }
        public int CurrentIndex => _currentIndex;
        
        object? IEnumerator.Current => Current;

        internal InstructionSetEnumerator(InstructionSet instructionSet)
        {
            _instructionSet = instructionSet;
            Reset();
        }
        
        public bool MoveNext()
        {
            if (!_hasStarted)
            {
                _hasStarted = true;
            }
            else
            {
                _currentIndex++;
            }
            
            SetCurrent();

            return Current != null;
        }

        /// <summary>
        /// Sets the current instruction to the instruction with the specified label
        /// </summary>
        /// <returns>
        /// Returns true if the labelled instruction is now being pointed at, or false if the label doesn't exist
        /// </returns>
        public bool MoveToLabel(string label)
        {
            if (!_instructionSet._labelIndices.TryGetValue(label.Trim(), out var index))
            {
                return false;
            }
            
            _currentIndex = index;
            _hasStarted = true;
            SetCurrent();
            return true;
        }

        public void Reset()
        {
            _hasStarted = false;
            _currentIndex = 0;
            SetCurrent();
        }
        
        public void Dispose()
        {
        }

        private void SetCurrent()
        {
            if (!_hasStarted || _currentIndex >= _instructionSet._instructions.Count)
            {
                Current = null;
            }
            else
            {
                Current = _instructionSet._instructions[_currentIndex];
            }
        }
    }
}