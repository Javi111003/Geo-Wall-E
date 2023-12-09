WALL-E!! GEO WALL-E Geometrical Asistant
===========================
## Colaboradores
- [Luis E. Amát Cárdenas]
- [Javier A. González Díaz]

## Cuál es el propósito de este software_
Es un intérprete para el lenguaje de programación G# que es capaz de recibir instrucciones para representar figuras bidimensionales en un área de dibujo.
  
## Requerimientos
Dotnet 6.0

## Portabilidad

Funciona en: Windows.

## Guía rápida
1. 

\geo_walle:dotnet run

## Testing
```
cd Tests
dotnet test     
```


## Cómo funciona?
1. El Lexer lee el texto enviado y lo convierte en tokens
2. El parser recibe los tokens y establece las relaciones entre las diferentes estructuras (árboles de sintaxis abstractos)
3. El intérprete recibe el árbol de sintaxis y lo evalúa pasándole el contexto global.Se serializan las representaciones geométricas por el Drawer
4. Se representan las figuras.
