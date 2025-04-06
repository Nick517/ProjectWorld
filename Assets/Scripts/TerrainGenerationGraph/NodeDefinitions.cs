using System.Collections.Generic;
using static TerrainGenerationGraph.NodeOperations;

namespace TerrainGenerationGraph
{
    public static class NodeDefinitions
    {
        public enum Type
        {
            Sample,
            Split,
            Float,
            Float2,
            Float3,
            Float4,
            Position,
            Negate,
            Add,
            Subtract,
            Multiply,
            Divide,
            Perlin3D
        }

        public static readonly Dictionary<Type, Definition> Definitions = new()
        {
            {
                Type.Sample, new Definition("Sample")
                {
                    InputPorts = new List<InputPort> { new() }
                }
            },
            {
                Type.Split, new Definition("Split", true)
                {
                    InputPorts = new List<InputPort> { new() },
                    OutputPorts = new List<OutputPort>
                    {
                        new("X", Operation.SplitOutX),
                        new("Y", Operation.SplitOutY),
                        new("Z", Operation.SplitOutZ),
                        new("W", Operation.SplitOutW)
                    }
                }
            },
            {
                Type.Float, new Definition("Float")
                {
                    InputPorts = new List<InputPort> { new("X") },
                    OutputPorts = new List<OutputPort> { new() }
                }
            },
            {
                Type.Float2, new Definition("Float 2")
                {
                    InputPorts = new List<InputPort>
                    {
                        new("X"),
                        new("Y")
                    },
                    OutputPorts = new List<OutputPort> { new(Operation.Float2, 2) }
                }
            },
            {
                Type.Float3, new Definition("Float 3")
                {
                    InputPorts = new List<InputPort>
                    {
                        new("X"),
                        new("Y"),
                        new("Z")
                    },
                    OutputPorts = new List<OutputPort> { new(Operation.Float3, 3) }
                }
            },
            {
                Type.Float4, new Definition("Float 4")
                {
                    InputPorts = new List<InputPort>
                    {
                        new("X"),
                        new("Y"),
                        new("Z"),
                        new("W")
                    },
                    OutputPorts = new List<OutputPort> { new(Operation.Float4, 4) }
                }
            },
            {
                Type.Position, new Definition("Position")
                {
                    OutputPorts = new List<OutputPort> { new(Operation.Position, 3) }
                }
            },
            {
                Type.Negate, new Definition("Negate")
                {
                    InputPorts = new List<InputPort> { new() },
                    OutputPorts = new List<OutputPort> { new(Operation.Negate) }
                }
            },
            {
                Type.Add, new Definition("Add", true)
                {
                    InputPorts = new List<InputPort>
                    {
                        new("A"),
                        new("B")
                    },
                    OutputPorts = new List<OutputPort> { new(Operation.Add) }
                }
            },
            {
                Type.Subtract, new Definition("Subtract", true)
                {
                    InputPorts = new List<InputPort>
                    {
                        new("A"),
                        new("B")
                    },
                    OutputPorts = new List<OutputPort> { new(Operation.Subtract) }
                }
            },
            {
                Type.Multiply, new Definition("Multiply", true)
                {
                    InputPorts = new List<InputPort>
                    {
                        new("A"),
                        new("B")
                    },
                    OutputPorts = new List<OutputPort> { new(Operation.Multiply) }
                }
            },
            {
                Type.Divide, new Definition("Divide", true)
                {
                    InputPorts = new List<InputPort>
                    {
                        new("A"),
                        new("B")
                    },
                    OutputPorts = new List<OutputPort> { new(Operation.Divide) }
                }
            },
            {
                Type.Perlin3D, new Definition("Perlin 3D")
                {
                    InputPorts = new List<InputPort>
                    {
                        new("Coord", 3),
                        new("Scale")
                    },
                    OutputPorts = new List<OutputPort> { new(Operation.Perlin3D) }
                }
            }
        };

        public class Definition
        {
            public readonly string Name;
            public readonly bool SetPortsToLowest;
            public List<InputPort> InputPorts = new();
            public List<OutputPort> OutputPorts = new();

            public Definition(string name, bool setPortsToLowest = false)
            {
                Name = name;
                SetPortsToLowest = setPortsToLowest;
            }
        }

        public class InputPort
        {
            public readonly string Name;
            public readonly int DefaultDimensions;

            public InputPort(string name = "In", int defaultDimensions = 1)
            {
                Name = name;
                DefaultDimensions = defaultDimensions;
            }
        }

        public class OutputPort
        {
            public readonly string Name;
            public readonly Operation Operation;
            public readonly int DefaultDimensions;

            public OutputPort(Operation operation, int defaultDimensions = 1)
            {
                Name = "Out";
                Operation = operation;
                DefaultDimensions = defaultDimensions;
            }

            public OutputPort(string name = "Out", Operation operation = default, int defaultDimensions = 1)
            {
                Name = name;
                Operation = operation;
                DefaultDimensions = defaultDimensions;
            }
        }
    }
}