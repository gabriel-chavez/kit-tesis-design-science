# -*- coding: utf-8 -*-
"""Combina los productos de tesis_ds en un solo documento Markdown."""
import os

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
TD = os.path.join(BASE, "tesis_ds")

ORDEN = [
    "00_estado.md",
    "01_areas_y_temas.md",
    "02_delimitacion.md",
    "03_perfil_ds.md",
    "04_marco_teorico.md",
    "05_estado_del_arte.md",
    "06_diagnostico.md",
    "06_anexo_linea_base_experimental.md",
    "07a_alternativas.md",
    "07b_requisitos.md",
    "07c_diseno_artefacto.md",
    "07d_plan_construccion.md",
    "07e_construccion_verificacion.md",
    "07e_doble_evaluacion.md",
    "07f_ficha_artefacto.md",
    "08_evaluacion.md",
    "09_enlace_solucion.md",
    "10_contribucion_conclusiones.md",
    "metodo.md",
    "referencias.md",
]

TITULOS = {
    "06_anexo_linea_base_experimental.md": "# Anexo E6. Línea base experimental",
    "07e_doble_evaluacion.md": "# Anexo E7e. Diseño del doble de evaluación",
}

partes = []
for nombre in ORDEN:
    ruta = os.path.join(TD, nombre)
    with open(ruta, "r", encoding="utf-8") as f:
        texto = f.read().strip()
    if nombre in TITULOS:
        lineas = texto.split("\n")
        lineas[0] = TITULOS[nombre]
        texto = "\n".join(lineas)
    partes.append(texto)

salida = "\n\n---\n\n".join(partes) + "\n"
destino = os.path.join(BASE, "entrega_alsie", "DSR_trabajo_original.md")
with open(destino, "w", encoding="utf-8") as f:
    f.write(salida)
print(destino)
print(f"Caracteres: {len(salida)}")
