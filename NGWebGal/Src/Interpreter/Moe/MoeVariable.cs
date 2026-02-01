using System;
using System.Collections.Generic;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// Represents a variable in the Moe script system
/// </summary>
public class MoeVariable
{
    public string Name { get; set; } = "";

    private List<int> _dimension = [];
    private bool _dirty = true;
    private object? _obj;

    public MoeVariableAccess Access { get; private set; } = MoeVariableAccess.Const;
    public MoeVariableType Type { get; private set; } = MoeVariableType.Void;
    public int Size { get; private set; } = 0;

    public List<int> Dimension
    {
        private get => _dimension;
        set
        {
            int totalSize = 1;
            if (value.Count == 0) value.Add(1);
            _dimension = value;
            foreach (var size in value) totalSize *= size;
            Size = totalSize;
            _dirty = true;
        }
    }

    public MoeVariable() { _dirty = true; }

    public MoeVariable(object obj)
    {
        Access = MoeVariableAccess.Const;
        if (obj is int ivalue)
        {
            Type = MoeVariableType.Int;
            Dimension = [1];
            this[0] = ivalue;
        }
        else if (obj is float fvalue)
        {
            Type = MoeVariableType.Float;
            Dimension = [1];
            this[0] = fvalue;
        }
        else if (obj is string svalue)
        {
            Type = MoeVariableType.String;
            Dimension = [1];
            this[0] = svalue;
        }
    }

    public MoeVariable(MoeVariableAccess access, MoeVariableType type)
    {
        Type = type;
        Access = access;
        _dirty = true;
    }

    private void Init()
    {
        _obj = Type switch
        {
            MoeVariableType.Int => new int[Size],
            MoeVariableType.Float => new float[Size],
            MoeVariableType.String => new string[Size],
            _ => new object[Size],
        };
        _dirty = false;
    }

    public override string ToString()
    {
        string ret = $"Name: {Name},\tAccess: {Access}: {Type},\tSize:";
        ret += (Dimension.Count < 0) ? "Error" : $"{Size} , \tDimension: [{string.Join(", ", Dimension)}]";
        return ret;
    }

    public void CloneFrom(MoeVariable variable)
    {
        Access = variable.Access;
        Type = variable.Type;
        List<int> dimen = [];
        foreach (var item in variable.Dimension)
            dimen.Add(item);
        Dimension = dimen;
        Init();
        for (int i = 0; i < Size; i++)
            this[i] = variable[i];
    }

    public void CopyFrom(MoeVariable variable)
    {
        Type = variable.Type;
        _dimension.Clear();
        foreach (var item in variable.Dimension)
            _dimension.Add(item);
        Init();
        for (int i = 0; i < Size; i++)
            this[i] = variable[i];
    }

    /// <summary>
    /// Multi-dimensional array indexer
    /// </summary>
    public object this[List<int> index]
    {
        get
        {
            if (_dirty) Init();
            if (_obj is null) throw new InvalidOperationException("Empty Object");
            if (index.Count == 0) index.Add(0);
            if (index.Count != _dimension.Count)
                throw new IndexOutOfRangeException($"{this} {index.Count}:{_dimension.Count}");

            int pos = 0;
            for (int i = 0; i < _dimension.Count; i++)
            {
                if (index[i] >= _dimension[i] || index[i] < 0)
                    throw new IndexOutOfRangeException();
                pos = pos * _dimension[i] + index[i];
            }

            return Type switch
            {
                MoeVariableType.Int => ((int[])_obj)[pos],
                MoeVariableType.Float => ((float[])_obj)[pos],
                MoeVariableType.String => ((string[])_obj)[pos],
                _ => throw new InvalidOperationException("Unknown type"),
            };
        }
        set
        {
            if (_dirty) Init();
            if (_obj is null) throw new InvalidOperationException("Empty Object");
            if (index.Count == 0) index.Add(0);
            if (index.Count != _dimension.Count)
                throw new IndexOutOfRangeException($"{this} {index.Count}:{_dimension.Count}");

            int pos = 0;
            for (int i = 0; i < _dimension.Count; i++)
            {
                if (index[i] >= _dimension[i] || index[i] < 0)
                    throw new IndexOutOfRangeException();
                pos = pos * _dimension[i] + index[i];
            }

            switch (Type)
            {
                case MoeVariableType.Int: ((int[])_obj)[pos] = (int)value; break;
                case MoeVariableType.Float: ((float[])_obj)[pos] = (float)value; break;
                case MoeVariableType.String: ((string[])_obj)[pos] = (string)value; break;
                default: throw new InvalidOperationException("Unknown type");
            }
        }
    }

    /// <summary>
    /// Single index accessor
    /// </summary>
    public object this[int index]
    {
        get
        {
            if (_dirty) Init();
            if (_obj is null) throw new InvalidOperationException("Empty Object");
            if (index < 0 || index >= Size) throw new IndexOutOfRangeException();

            return Type switch
            {
                MoeVariableType.Int => ((int[])_obj)[index],
                MoeVariableType.Float => ((float[])_obj)[index],
                MoeVariableType.String => ((string[])_obj)[index],
                _ => throw new InvalidOperationException("Unknown type"),
            };
        }
        set
        {
            if (_dirty) Init();
            if (_obj is null) throw new InvalidOperationException("Empty Object");
            if (index < 0 || index >= Size) throw new IndexOutOfRangeException();

            switch (Type)
            {
                case MoeVariableType.Int: ((int[])_obj)[index] = (int)value; break;
                case MoeVariableType.Float: ((float[])_obj)[index] = (float)value; break;
                case MoeVariableType.String: ((string[])_obj)[index] = (string)value; break;
                default: throw new InvalidOperationException("Unknown type");
            }
        }
    }

    // Implicit conversions
    public static implicit operator int(MoeVariable v) =>
        (v.Type == MoeVariableType.Int && v._obj is not null) ? (int)v[0] : 0;
    public static implicit operator float(MoeVariable v) =>
        (v.Type == MoeVariableType.Float && v._obj is not null) ? (float)v[0] : 0.0f;
    public static implicit operator string(MoeVariable v) =>
        (v.Type == MoeVariableType.String && v._obj is not null) ? (string)v[0] : "";

    public static implicit operator MoeVariable(int value)
    {
        var ret = new MoeVariable(MoeVariableAccess.Const, MoeVariableType.Int) { Dimension = [1] };
        ret[0] = value;
        return ret;
    }

    public static implicit operator MoeVariable(float value)
    {
        var ret = new MoeVariable(MoeVariableAccess.Const, MoeVariableType.Float) { Dimension = [1] };
        ret[0] = value;
        return ret;
    }

    public static implicit operator MoeVariable(string value)
    {
        var ret = new MoeVariable(MoeVariableAccess.Const, MoeVariableType.String) { Dimension = [1] };
        ret[0] = value;
        return ret;
    }
}
