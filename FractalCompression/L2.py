import math, struct
import random
import sys
from PIL import Image, ImageDraw
import BlockClass

# Класс для подсчета сдвига по яркости и отклонения
class L2:

        # Сдвиг по яркости (можно ускорить если считать значения всех пикселей блока на этапе инициализации)
        def Shift(self, RangeBlock, DomainBlock):
            width = RangeBlock.Blockimage.size[0]  # Определяем ширину.
            height = RangeBlock.Blockimage.size[1]  # Определяем высоту.
            RangePixels = RangeBlock.Blockimage.load()  # Выгружаем значения пикселей.
            DomainPixels = DomainBlock.Blockimage.load()
            # Вычисление сдвига по яркости
            DomainValue = 0
            RangeValue = 0
            for i in range(width):
                for j in range(height):
                    # Доменные пиксели берем через один
                    R = DomainPixels[i * 2, j * 2][0]
                    G = DomainPixels[i * 2, j * 2][1]
                    B = DomainPixels[i * 2, j * 2][2]
                    DomainValue = DomainValue + 0.75 * (R + G + B)

                    # Значения пикселей рангового блока
                    R = RangePixels[i, j][0]
                    G = RangePixels[i, j][1]
                    B = RangePixels[i, j][2]
                    RangeValue = RangeValue + (R + G + B)

            Shift = (DomainValue - RangeValue) / (width * height)
            return Shift

        # Среднеквадратичное отклонение
        def Distance(self, RangeBlock, DomainBlock, shift):
            # Среднеквадратичное отклонения
            Dist = 0
            width = RangeBlock.Blockimage.size[0]  # Определяем ширину.
            height = RangeBlock.Blockimage.size[1]  # Определяем высоту.
            RangePixels = RangeBlock.Blockimage.load()  # Выгружаем значения пикселей.
            DomainPixels = DomainBlock.Blockimage.load()
            # Вычисление cреднеквадратичного отклонения
            for i in range(width):
                for j in range(height):
                    # Перебираем все значения ранговых пикселей
                    R = RangePixels[i, j][0]
                    G = RangePixels[i, j][1]
                    B = RangePixels[i, j][2]
                    RangeValue = (R + G + B)

                    # Доменные пиксели берем через один
                    R = DomainPixels[i * 2, j * 2][0]
                    G = DomainPixels[i * 2, j * 2][1]
                    B = DomainPixels[i * 2, j * 2][2]
                    DomainValue = (R + G + B)

                    #Dist = Dist + (RangeValue + self.Shift(RangeBlock, DomainBlock) - 0.75 * DomainValue) ** 2
                    Dist = Dist + (RangeValue + shift - 0.75 * DomainValue) ** 2
            return Dist
