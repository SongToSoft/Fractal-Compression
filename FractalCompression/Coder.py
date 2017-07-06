import math, struct
import random
import sys
from PIL import Image, ImageDraw
import BlockClass
import L2

#Класс, описывающий алгоритм сжатия.
class Coder:

    def Compression(self, string, N):
        # Открываем изображение.
        image = Image.open(string)
        #image.show()
        width = image.size[0]  # Определяем ширину.
        height = image.size[1]  # Определяем высоту.
        # newimage = Image.new("RGB", (width, height), (0, 0, 0))
        # draw = ImageDraw.Draw(newimage)  # Создаем инструмент для рисования.
        # pixel = image.load()  # Выгружаем значения пикселей.
        # print(width)
        # print(height)
        outFile = open("Compressed file.txt", "w")
        range_size = N
        outFile.write(str(width) + ' ' + str(height) + '\n')
        outFile.write(str(range_size) + '\n')
        range_num_width = width // range_size
        range_num_height = height // range_size


        domain_size = range_size * 2
        domain_num_width = width // domain_size
        domain_num_height = height // domain_size

        RangeBlockList = []
        for i in range(range_num_width):
            for j in range(range_num_height):
                # Создаем ранговые блоки
                RangeBlock = BlockClass.Block(image, range_size, i, j, 0)
                # RangeBlock.Show()
                RangeBlockList.append(RangeBlock)
        RangeBlockList.reverse() #Разворачиваем список

        DomainBlockList = []
        for i in range(domain_num_width):
            for j in range(domain_num_height):
                for k in range(8):
                    # Создаем доменные блоки со всеми вариантами симетрии
                    DomainBlock = BlockClass.Block(image, domain_size, i, j, k)
                    #DomainBlock.Show()
                    DomainBlockList.append(DomainBlock)
        DomainBlockList.reverse()


        RangeBlockListCopy = RangeBlockList.copy()
        while(RangeBlockListCopy):
            RangeBlock = RangeBlockListCopy.pop()
            DomainBlockListCopy = DomainBlockList.copy()
            min_distance = sys.maxsize
            while(DomainBlockListCopy):
                DomainBlock = DomainBlockListCopy.pop()
                # Запоминаем координаты текущего преобразования
                current_coor_x = DomainBlock.coor_x
                current_coor_y = DomainBlock.coor_y
                current_coeff = DomainBlock.coeff
                current_shift = L2.L2().Shift(RangeBlock, DomainBlock)
                current_distance = L2.L2().Distance(RangeBlock, DomainBlock, current_shift)
                if (current_distance < min_distance):
                    min_distance = current_distance
                    best_coor_x = current_coor_x
                    best_coor_y = current_coor_y
                    best_coeff = current_coeff
                    best_shift = current_shift
            outFile.write(str(best_coor_x) + ' ' + str(best_coor_y) + ' ' + str(best_coeff) + ' ' + str(best_shift) + '\n')
            print("!!!!!!!")
            print(best_coor_x)
            print(best_coor_y)
            print(best_coeff)
            print(best_shift)
            print("!!!!!!!")

        outFile.close()
    def Decompression(self):
        print("Begin")




