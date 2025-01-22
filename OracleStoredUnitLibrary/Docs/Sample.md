# UnitTestSample

## FUNCTION

### F_IS_NUMBER

```
CREATE OR REPLACE FUNCTION F_IS_NUMBER(PRAM VARCHAR2) RETURN BOOLEAN IS

-- 引数が数値であればTRUE、それ以外ならFALSEを返す関数;
BEGIN
    IF REGEXP_LIKE(PRAM, '^[0-9]+$') THEN
        RETURN TRUE;
    ELSE
        RETURN FALSE;
    END IF;
END F_IS_NUMBER;
```

### F_GET_CURRENT_YEAR
```
CREATE OR REPLACE FUNCTION get_current_year
RETURN NUMBER
IS
  current_year NUMBER;
BEGIN
  SELECT TO_NUMBER(TO_CHAR(SYSDATE, 'YYYY')) INTO current_year FROM DUAL;

  -- 現在の年を返す
  RETURN current_year;
END get_current_year;
```

## PROCEDURE

### P_INSERT_INTO_TABLE_A
```
CREATE OR REPLACE PROCEDURE P_INSERT_INTO_TABLE_A (
  p_id          IN NUMBER,
  p_name        IN VARCHAR2
)
IS
BEGIN

  INSERT INTO table_a (id, name, created_date)
  VALUES (p_id, p_name, SYSDATE);

EXCEPTION
  WHEN OTHERS THEN
    RAISE;
END P_INSERT_INTO_TABLE_A;
```

###
```
CREATE OR REPLACE PROCEDURE P_RETURN_OUT (
    p_number IN NUMBER,
    p_square OUT NUMBER
) AS
BEGIN
    p_square := p_number * p_number;
END;
```