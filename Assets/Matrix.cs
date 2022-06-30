using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Matrix<T> : IEnumerable<T>
{
    //IMPLEMENTAR: ESTRUCTURA INTERNA- DONDE GUARDO LOS DATOS?*
    private T[,] _board;

    public Matrix(int width, int height)
    {
        //IMPLEMENTAR: constructor*
        _board = new T[width,height];
        Width = width;
        Height = height;
    }

	public Matrix(T[,] copyFrom)
    {
        //IMPLEMENTAR: crea una version de Matrix a partir de una matriz básica de C#*
        _board = new T[copyFrom.GetLength(0), copyFrom.GetLength(1)];
        Width = copyFrom.GetLength(0);
        Height = copyFrom.GetLength(1);

        for (int i = 0; i < copyFrom.GetLength(0); i++)
        {
            for (int j = 0; j < copyFrom.GetLength(1); j++)
            {
                _board[i, j] = copyFrom[i,j];
            }
        }
    }

	public Matrix<T> Clone() {
        //IMPLEMENTAR*

        Matrix<T> ClonedMatrix = new Matrix<T>(Width, Height);

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                ClonedMatrix._board[i, j] = _board[i, j];
            }
        }
        return ClonedMatrix;
    }

	public void SetRangeTo(int x0, int y0, int x1, int y1, T item) {
        //IMPLEMENTAR: iguala todo el rango pasado por parámetro a item*
        for (int i = y0; i < y1; i++)
        {
            for (int j = x0; j < x1; j++)
            {
                _board[j, i] = item;
            }
        }

    }

    //Todos los parametros son INCLUYENTES*
    public List<T> GetRange(int x0, int y0, int x1, int y1) {
        List<T> l = new List<T>();
        for (int i = y0; i < y1; i++)
        {
            for (int j = x0; j < x1; j++)
            {
                l.Add(_board[j, i]);
            }
        }
        //IMPLEMENTAR*
        return l;
	}

    //Para poder igualar valores en la matrix a algo*
    public T this[int x, int y] {
		get
        {
            //IMPLEMENTAR*
            return _board[x,y];
		}
		set {
            //IMPLEMENTAR*
            _board[x, y] = value;

        }
	}

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int Capacity { get; private set; }

    public IEnumerator<T> GetEnumerator()
    {
        //IMPLEMENTAR*
        for (int i = 0; i < _board.GetLength(0); i++)
        {
            for (int j = 0; j < _board.GetLength(1); j++)
            {
                yield return _board[i, j];
            }
        }
        
    }

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
    }
}
