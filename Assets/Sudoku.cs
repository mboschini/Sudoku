using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class Sudoku : MonoBehaviour {
	public Cell prefabCell;
	public Canvas canvas;
	public Text feedback;
	public float stepDuration = 0.1f;
	[Range(1, 82)]public int difficulty = 5;

	Matrix<Cell> _board;
	Matrix<int> _createdMatrix;
    List<int> posibles = new List<int>();
	int _smallSide;
	int _bigSide;
    string memory = "";
    string canSolve = "";
    bool canPlayMusic = false;
    List<int> nums = new List<int>();


    float r = 1.0594f;
    float frequency = 440;
    float gain = 0.5f;
    float increment;
    float phase;
    float samplingF = 48000;
    float maxCount;

    void Start()
    {
        long mem = System.GC.GetTotalMemory(true);
        feedback.text = string.Format("MEM: {0:f2}MB", mem / (1024f * 1024f));
        memory = feedback.text;
        _smallSide = 3;
        _bigSide = _smallSide * 3;
        frequency = frequency * Mathf.Pow(r, 2);
        CreateEmptyBoard();
        ClearBoard();
    }

    void ClearBoard() {
		_createdMatrix = new Matrix<int>(_bigSide, _bigSide);
		foreach(var cell in _board) {
			cell.number = 0;
			cell.locked = cell.invalid = false;
		}
	}

	void CreateEmptyBoard() {
		float spacing = 68f;
		float startX = -spacing * 4f;
		float startY = spacing * 4f;

		_board = new Matrix<Cell>(_bigSide, _bigSide);
		for(int x = 0; x<_board.Width; x++) {
			for(int y = 0; y<_board.Height; y++) {
                var cell = _board[x, y] = Instantiate(prefabCell);
				cell.transform.SetParent(canvas.transform, false);
				cell.transform.localPosition = new Vector3(startX + x * spacing, startY - y * spacing, 0);
			}
		}
	}
	


	//IMPLEMENTAR
	int watchdog = 0;
	bool RecuSolve(Matrix<int> matrixParent, int x, int y, int protectMaxDepth, List<Matrix<int>> solution)
    {
        if (y == 9)
        {
            y = 0; ++x;
            if (x == 9) return true;
        }

        if (_board[x,y].locked == true)
        {
            y = y + 1;
            return RecuSolve(matrixParent, x, y,protectMaxDepth,solution);
        }
            

        for (int num = 1; num <= 9; num++)
        {
            if (CanPlaceValue(matrixParent,num, x, y))
            {
                _board[x,y].number = num;
                matrixParent[x, y] = num;
                solution.Add(matrixParent.Clone());

                if (RecuSolve(matrixParent, x, y+1, protectMaxDepth, solution)) 
                    return true;
                else
                {
                    _board[x,y].number = 0;
                    matrixParent[x,y] = 0;
                    solution.RemoveAt(solution.Count-1);
                }
            }
        }

        return false;
    }


    void OnAudioFilterRead(float[] array, int channels)
    {
        if(canPlayMusic)
        {
            increment = frequency * Mathf.PI / samplingF;
            for (int i = 0; i < array.Length; i++)
            {
                phase = phase + increment;
                array[i] = (float)(gain * Mathf.Sin((float)phase));
            }
        }
        
    }
    void changeFreq(int num)
    {
        frequency = 440 + num * 80;
    }

	//IMPLEMENTAR - punto 3
	IEnumerator ShowSequence(List<Matrix<int>> seq)
    {
        yield return new WaitForSeconds(stepDuration);

        if (seq.Count > 0)
        {
            TranslateAllValuesNonSetter(seq[0]);
            seq.RemoveAt(0);
            feedback.text = "Pasos: " + (maxCount - seq.Count).ToString() + "/" + maxCount + " - " + memory + " - " + canSolve;
            StartCoroutine(ShowSequence(seq));
        }
    }

	void Update () {
		if(Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(1))
            SolvedSudoku();
        else if(Input.GetKeyDown(KeyCode.C) || Input.GetMouseButtonDown(0)) 
            CreateSudoku();          
    }

	//modificar lo necesario para que funcione.
    void SolvedSudoku()
    {
        StopAllCoroutines();
        nums = new List<int>();
        var solution = new List<Matrix<int>>();
        watchdog = 100000;

        solution.Add(_createdMatrix.Clone());
        CreateNew();
        
        if (ValidBoard(_createdMatrix))
        {
            var result = RecuSolve(_createdMatrix, 0, 0, 10, solution);
            canSolve = result ? " VALID" : " INVALID";
            maxCount = solution.Count;
            StartCoroutine(ShowSequence(solution));
            long mem = System.GC.GetTotalMemory(true);
            memory = string.Format("MEM: {0:f2}MB", mem / (1024f * 1024f));
        }
        else
        {
            feedback.text = "INVALID";
        }
    }

    void CreateSudoku()
    {
        StopAllCoroutines();
        nums = new List<int>();
        canPlayMusic = false;
        ClearBoard();
        List<Matrix<int>> l = new List<Matrix<int>>();
        watchdog = 100000;

        GenerateValidLine(_createdMatrix, 0, 0);
        l.Add(_createdMatrix);
        TranslateAllValues(_createdMatrix);

        var result = RecuSolve(_createdMatrix, 0, 0, 10, l);

        AllUnlocked();
        LockRandomCells();
        ClearUnlocked();

        long mem = System.GC.GetTotalMemory(true);
        memory = string.Format("MEM: {0:f2}MB", mem / (1024f * 1024f));
        canSolve = result ? " VALID" : " INVALID";

        feedback.text = "Pasos: " + l.Count + "/" + l.Count + " - " + memory + " - " + canSolve;
    }
	void GenerateValidLine(Matrix<int> mtx, int x, int y)
	{
		int[]aux = new int[9];
		for (int i = 0; i < 9; i++) 
		{
			aux [i] = i + 1;
		}
		int numAux = 0;
		for (int j = 0; j < aux.Length; j++) 
		{
			int r = 1 + Random.Range(j,aux.Length);
			numAux = aux [r-1];
			aux [r-1] = aux [j];
			aux [j] = numAux;
		}
		for (int k = 0; k < aux.Length; k++) 
		{
			mtx [k, 0] = aux [k];
		}
	}


	void ClearUnlocked()
	{
		for (int i = 0; i < _board.Height; i++) {
			for (int j = 0; j < _board.Width; j++) {
				if (!_board [j, i].locked)
                    _board[j,i].number = Cell.EMPTY;
			}
		}
	}

    void AllUnlocked()
    {
        for (int i = 0; i < _board.Height; i++)
        {
            for (int j = 0; j < _board.Width; j++)
            {
                if (_board[j, i].locked)
                    _board[j, i].locked = false;
            }
        }
    }

    void LockRandomCells()
	{
		List<Vector2> posibles = new List<Vector2> ();
		for (int i = 0; i < _board.Height; i++) {
			for (int j = 0; j < _board.Width; j++) {
				if (!_board [j, i].locked)
					posibles.Add (new Vector2(j,i));
			}
		}
		for (int k = 0; k < 82-difficulty; k++) {
			int r = Random.Range (0, posibles.Count);
			_board [(int)posibles [r].x, (int)posibles [r].y].locked = true;
			posibles.RemoveAt (r);
		}
	}

    void TranslateAllValues(Matrix<int> matrix)
    {
        for (int y = 0; y < _board.Height; y++)
        {
            for (int x = 0; x < _board.Width; x++)
            {
                _board[x, y].number = matrix[x, y];
                if (_board[x, y].number == 0)
                    _board[x, y].locked = false;
                else
                    _board[x, y].locked = true;
            }
        }
    }

    void TranslateAllValuesNonSetter(Matrix<int> matrix)
    {
        for (int y = 0; y < _board.Height; y++)
        {
            for (int x = 0; x < _board.Width; x++)
            {
                _board[x, y].number = matrix[x, y];
            }
        }
    }

    void TranslateSpecific(int value, int x, int y)
    {
        _board[x, y].number = value;
        if (_board[x, y].number == 0)
            _board[x, y].locked = false;
        else
            _board[x, y].locked = true;
    }

    void TranslateRange(int x0, int y0, int xf, int yf)
    {
        for (int x = x0; x < xf; x++)
        {
            for (int y = y0; y < yf; y++)
            {
                _board[x, y].number = _createdMatrix[x, y];
                if (_board[x, y].number == 0)
                    _board[x, y].locked = false;
                else
                    _board[x, y].locked = true;
            }
        }
    }
    void CreateNew()
    {
        //_createdMatrix = new Matrix<int>(Tests.validBoards[2]);
        _createdMatrix = new Matrix<int>(Tests.validBoards[Random.Range(0, Tests.validBoards.Length)]);
        //_createdMatrix = new Matrix<int>(Tests.invalidBoards[Random.Range(0, Tests.invalidBoards.Length)]);

        TranslateAllValues(_createdMatrix);
    }

    bool CanPlaceValue(Matrix<int> mtx, int value, int x, int y)
    {
        List<int> fila = new List<int>();
        List<int> columna = new List<int>();
        List<int> area = new List<int>();
        List<int> total = new List<int>();

        Vector2 cuadrante = Vector2.zero;

        for (int i = 0; i < mtx.Height; i++)
        {
            for (int j = 0; j < mtx.Width; j++)
            {
                if (i != y && j == x) columna.Add(mtx[j, i]);
                else if(i == y && j != x) fila.Add(mtx[j,i]);
            }
        }

        cuadrante.x = (int)(x / 3);

        if (x < 3)
            cuadrante.x = 0;     
        else if (x < 6)
            cuadrante.x = 3;
        else
            cuadrante.x = 6;

        if (y < 3)
            cuadrante.y = 0;
        else if (y < 6)
            cuadrante.y = 3;
        else
            cuadrante.y = 6;
         
        area = mtx.GetRange((int)cuadrante.x, (int)cuadrante.y, (int)cuadrante.x + 3, (int)cuadrante.y + 3);
        total.AddRange(fila);
        total.AddRange(columna);
        total.AddRange(area);
        total = FilterZeros(total);

        if (total.Contains(value))
            return false;
        else
            return true;
    }


    List<int> FilterZeros(List<int> list)
    {
        List<int> aux = new List<int>();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != 0) aux.Add(list[i]);
        }
        return aux;
    }

    bool ValidFila(int fila, Matrix<int> mtx)
    {
        List<int> takenValues = new List<int>();
        List<int> s;
        int aux;

        for (int i = 0; i < 9; i++)
        {
            s = mtx.GetRange(0, fila, 9, fila + 1);
            takenValues.Clear();

            for (int j = 0; j < 9; j++)
            {
                aux = s[0];
                if (aux != 0)
                {
                    if (takenValues.Contains(aux))
                    {
                        return false;
                    }
                    else
                    {
                        takenValues.Add(aux);
                        s.RemoveAt(0);
                    }
                }
                else
                {
                    s.RemoveAt(0);
                }
            }
        }
        return true;
    }

    bool ValidCol(int col, Matrix<int> mtx)
    {
        List<int> takenValues = new List<int>();
        List<int> s;
        int aux;

        for (int i = 0; i < 9; i++)
        {
            s = mtx.GetRange(col, 0, col + 1, 9);
            takenValues.Clear();

            for (int j = 0; j < 9; j++)
            {
                aux = s[0];
                if (aux != 0)
                {
                    if (takenValues.Contains(aux))
                    {
                        return false;
                    }
                    else
                    {
                        takenValues.Add(aux);
                        s.RemoveAt(0);
                    }
                }
                else
                {
                    s.RemoveAt(0);
                }
            }
        }
        return true;
    }

    bool ValidCuads(Matrix<int> mtx) 
    {
        List<int> s = new List<int>();
        List<int> takenValues = new List<int>();
        int aux;

        for (int fila = 0; fila < 9; fila = fila + 3)
        {
            for (int col = 0; col < 9; col = col + 3)
            {
                s = mtx.GetRange(col, fila, col+3, fila + 3);
                takenValues.Clear();

                for (int i = 0; i < 9; i++)
                {
                    aux = s[0];
                    if (aux != 0)
                    {
                        if (takenValues.Contains(aux))
                        {
                            return false;
                        }
                        else
                        {
                            takenValues.Add(aux);
                            s.RemoveAt(0);
                        }
                    }
                    else
                    {
                        s.RemoveAt(0);
                    }
                }
            }
        }
        return true;
    }


    bool ValidBoard(Matrix<int> mtx)
    {
        for (int i = 0; i < 9; i++)
        {
            if (!ValidFila(i, mtx) || !ValidCol(i, mtx))
            {
                return false;
            }
        }

        if (ValidCuads(mtx))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
