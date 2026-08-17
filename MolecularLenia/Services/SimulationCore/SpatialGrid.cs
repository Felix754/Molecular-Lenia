using MolecularLenia.Models;
using System;
using System.Collections.Generic;

namespace MolecularLenia.Services
{
    public class SpatialGrid
    {
        public List<Particle>[,] Cells { get; private set; } = null!;
        public int Cols { get; private set; }
        public int Rows { get; private set; }
        public float CellSize { get; private set; }

        public void Build(List<Particle> particles, float cellSize)
        {
            int newCols = (int)MathF.Ceiling(1920f / cellSize) + 1;
            int newRows = (int)MathF.Ceiling(1080f / cellSize) + 1;

            // If the grid has not been created yet or interaction radius changed -- allocate memory
            if (Cells == null || Cols != newCols || Rows != newRows)
            {
                Cols = newCols;
                Rows = newRows;
                CellSize = cellSize;
                Cells = new List<Particle>[Cols, Rows];

                for (int x = 0; x < Cols; x++)
                {
                    for (int y = 0; y < Rows; y++)
                    {
                        Cells[x, y] = new List<Particle>();
                    }
                }
            }
            else
            {
                // HACK: Grid already exists. Just clear existing lists (Zero Allocations)
                CellSize = cellSize;
                for (int x = 0; x < Cols; x++)
                {
                    for (int y = 0; y < Rows; y++)
                    {
                        Cells[x, y].Clear();
                    }
                }
            }

            // Distribute particles across predefined cells
            foreach (var p in particles)
            {
                int c = Math.Clamp((int)(p.X / CellSize), 0, Cols - 1);
                int r = Math.Clamp((int)(p.Y / CellSize), 0, Rows - 1);
                Cells[c, r].Add(p);
            }
        }
    }
}