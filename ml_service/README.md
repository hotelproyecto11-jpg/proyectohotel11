ML prototipo para sugerencias de precio

Cómo usar (entorno Windows PowerShell):

1) Crear entorno virtual e instalar dependencias

```powershell
cd ml_service
python -m venv venv
venv\Scripts\Activate
pip install -r requirements.txt
```

2) Ejecutar el servicio

```powershell
uvicorn main:app --reload --port 8000
```

3) Entrenar con un POST /train enviando JSON con la propiedad `rows` (lista de registros).
4) Consultar POST /predict para obtener `predictedPrice`.

Nota: este es un prototipo rápido para iteración. Para producción conviene añadir validación, versionado de modelos y secure endpoints.
