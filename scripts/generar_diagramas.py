# -*- coding: utf-8 -*-
"""Genera las figuras del IFI (modelo y metodo) como PNG."""
import os
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch, Rectangle
from matplotlib.lines import Line2D

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
IMG = os.path.join(BASE, "entrega_alsie", "img")
os.makedirs(IMG, exist_ok=True)

AZUL = "#1F3864"
AZUL_CLARO = "#DEEAF6"
NARANJA = "#C55A11"
GRIS = "#595959"


def caja(ax, x, y, w, h, texto, color=AZUL_CLARO, borde=AZUL, fs=10, bold=False):
    p = FancyBboxPatch((x - w / 2, y - h / 2), w, h,
                       boxstyle="round,pad=0.04,rounding_size=0.10",
                       linewidth=1.4, edgecolor=borde, facecolor=color)
    ax.add_patch(p)
    ax.text(x, y, texto, ha="center", va="center", fontsize=fs,
            color="#111111", fontweight="bold" if bold else "normal",
            wrap=True)


def flecha(ax, x1, y1, x2, y2, color=AZUL, style="-|>", lw=1.5, ls="-", rad=0.0):
    a = FancyArrowPatch((x1, y1), (x2, y2), arrowstyle=style,
                        mutation_scale=14, linewidth=lw, color=color,
                        linestyle=ls, connectionstyle=f"arc3,rad={rad}")
    ax.add_patch(a)


def figura_modelo():
    fig, ax = plt.subplots(figsize=(13, 7.2), dpi=200)
    ax.set_xlim(0, 13)
    ax.set_ylim(0, 7.2)
    ax.axis("off")

    # Fila superior
    caja(ax, 1.9, 5.7, 2.9, 1.15,
         "C1. Identidad de la operación\n(llave estable y reproducible)", fs=10.5, bold=False)
    caja(ax, 6.5, 5.7, 3.1, 1.15,
         "C2. Estado del participante externo\n(categoría + existencia del recurso)", fs=10.5)
    caja(ax, 11.1, 5.7, 2.9, 1.15,
         "C3. Decisión\n(reprocesar / compensar /\nreconciliar)", fs=10.5)

    # Fila inferior
    caja(ax, 11.1, 2.3, 2.9, 1.15,
         "C4. Identidad y reserva\nde la compensación\n(operación, motivo)", fs=10.5)
    caja(ax, 6.5, 2.3, 3.1, 1.15,
         "C5. Verificación contra\nel efecto observable\n(control negativo)", fs=10.5)

    caja(ax, 1.9, 2.3, 2.6, 0.95,
         "Estado final\nconfirmado", color="#E2F0D9", borde="#548235", fs=10.5)

    # Flechas horizontales
    flecha(ax, 3.4, 5.7, 4.9, 5.7)
    ax.text(4.15, 6.0, "R1", fontsize=9, color=NARANJA, fontweight="bold")
    flecha(ax, 8.1, 5.7, 9.6, 5.7)
    ax.text(8.85, 6.0, "R2", fontsize=9, color=NARANJA, fontweight="bold")

    # C3 -> C4
    flecha(ax, 11.1, 5.1, 11.1, 2.9)
    ax.text(11.35, 4.0, "R3", fontsize=9, color=NARANJA, fontweight="bold")

    # C4 -> C5
    flecha(ax, 9.6, 2.3, 8.1, 2.3)
    ax.text(8.85, 2.6, "R4", fontsize=9, color=NARANJA, fontweight="bold")

    # C5 -> estado final
    flecha(ax, 4.9, 2.3, 3.25, 2.3)

    # C5 -> C2 (realimentación)
    flecha(ax, 6.5, 2.9, 6.5, 5.1, color=NARANJA, ls="--", rad=-0.35)
    ax.text(5.2, 4.0, "R5: la verificación\nalimenta la reconciliación",
            fontsize=9, color=NARANJA, ha="center")

    # Etiqueta ventana
    ax.text(11.1, 0.95, "Ventana declarada para lo indeterminado:\nreconsultar y, si persiste, reconciliar",
            ha="center", va="center", fontsize=9, color=GRIS, style="italic")

    # Título interno
    ax.text(0.4, 6.85, "Modelo de idempotencia de la compensación",
            fontsize=13, fontweight="bold", color=AZUL, ha="left")

    fig.tight_layout()
    out = os.path.join(IMG, "modelo.png")
    fig.savefig(out, bbox_inches="tight", facecolor="white")
    plt.close(fig)
    return out


def figura_metodo():
    fig, ax = plt.subplots(figsize=(10, 8.2), dpi=200)
    ax.set_xlim(0, 10)
    ax.set_ylim(0, 8.4)
    ax.axis("off")

    pasos = [
        ("1. K1 · Identidad de la operación", "llave derivada de datos estables"),
        ("2. K2 · Bitácora de estado", "registro previo a la llamada"),
        ("3. Ejecutar en el participante externo", "la cadena completa"),
        ("4. K3 · Consultar el estado real", "solo ante sospecha: timeout o pérdida"),
        ("5. K4 · Decidir con dos hechos", "categoría de negocio + existencia del recurso"),
        ("6. K5 / K6 / K7 · Compensar o reconciliar", "par (operación, motivo), orden inverso, ventana"),
        ("7. K8 · Verificar contra el efecto", "control negativo y contadores del externo"),
    ]
    y0 = 7.35
    dy = 0.93
    for i, (titulo, sub) in enumerate(pasos):
        y = y0 - i * dy
        color = AZUL_CLARO if i not in (3, 4, 5) else "#FBE5D6"
        caja(ax, 4.6, y, 5.6, 0.68, "", color=color)
        ax.text(4.6, y + 0.10, titulo, ha="center", va="center",
                fontsize=10.2, fontweight="bold", color="#111111")
        ax.text(4.6, y - 0.17, sub, ha="center", va="center",
                fontsize=8.6, color=GRIS, style="italic")
        if i < len(pasos) - 1:
            flecha(ax, 4.6, y - 0.34, 4.6, y - dy + 0.34)

    # Bucle de reintento / reconsulta
    flecha(ax, 7.4, 3.15, 7.4, 5.95, color=NARANJA, ls="--", rad=0.55)
    ax.text(8.55, 4.55, "reintento o\nreconsulta", fontsize=9, color=NARANJA,
            ha="center", rotation=90)

    # Nota lateral
    ax.text(0.25, 0.55,
            "La consulta (paso 4) se ejecuta ante timeout o respuesta perdida;\n"
            "la compensación (paso 6) solo si el estado confirma el efecto y existe acción disponible.",
            fontsize=8.8, color=GRIS, ha="left", va="bottom", style="italic")

    ax.text(0.25, 8.15, "Método de compensación idempotente",
            fontsize=13, fontweight="bold", color=AZUL, ha="left")

    fig.tight_layout()
    out = os.path.join(IMG, "metodo.png")
    fig.savefig(out, bbox_inches="tight", facecolor="white")
    plt.close(fig)
    return out


if __name__ == "__main__":
    print(figura_modelo())
    print(figura_metodo())
