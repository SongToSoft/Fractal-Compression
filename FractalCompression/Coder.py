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
        outFile = open("Compressed file Triangle.txt", "w")
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

        # Компрессия
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

        outFile.close()

    def Decompression(self):
        decFile = open("Compressed file Triangle.txt", "r")
        # Считываем все коэффиценты из файла
        String = decFile.read().replace("\n", " ").split(" ")

        count = 0
        width = (int)(String[count])
        count = count + 1
        height = (int)(String[count])
        count = count + 1

        range_size = (int)(String[count])
        count = count + 1

        # Создаём изображение
        newimage = Image.new("RGB", (width, height), (255, 255, 255))
        #newimage = Image.open("Expanded file.png")
        range_num_width = width // range_size
        range_num_height = height // range_size


        domain_size = range_size * 2
        #domain_num_width = width // domain_size
        #domain_num_height = height // domain_size

        # Разбиваем созданное изображение на ранговые блоки
        RangeBlockList = []
        for i in range(range_num_width):
            for j in range(range_num_height):
                # Создаем ранговые блоки
                RangeBlock = BlockClass.Block(newimage, range_size, i, j, 0)
                #RangeBlock.Show()
                RangeBlockList.append(RangeBlock)
        RangeBlockList.reverse()  # Разворачиваем список

        # Создаем доменные блоки по коэффицентам из сжатого файла
        DomainBlockList = []
        for i in range(range_num_width):
            for j in range(range_num_height):
                current_x = (int)(String[count])
                count = count + 1
                current_y = (int)(String[count])
                count = count + 1
                current_rotate = (int)(String[count])
                count = count + 1
                current_shift = (float)(String[count])
                count = count + 1
                #print(str(current_x) + ' ' + str(current_y) + ' ' + str(current_rotate) + ' ' + str(current_shift))
                DomainBlock = BlockClass.Block(newimage, domain_size, current_y, current_x, current_rotate)
                DomainBlock.SetShift(current_shift)
                #DomainBlock.Show()
                DomainBlockList.append(DomainBlock)
        DomainBlockList.reverse()

        # Декомпрессия
        RangeBlockListCopy = RangeBlockList.copy()
        DomainBlockListCopy = DomainBlockList.copy()
        FinalRangeBlockList = []
        while(RangeBlockListCopy):
            RangeBlock = RangeBlockListCopy.pop()
            DomainBlock = DomainBlockListCopy.pop()
            #DomainBlock.Show()
            # RangePixels = RangeBlock.Blockimage.load()  # Выгружаем значения пикселей.
            DomainPixels = DomainBlock.Blockimage.load()
            Bufferimage = Image.new("RGB", (range_size, range_size), (255, 255, 255))
            draw = ImageDraw.Draw(Bufferimage)  # Создаем инструмент для рисования.
            for i in range(range_size):
                for j in range(range_size):
                    R = (int)(0.75 * (DomainPixels[i * 2, j * 2][0]) + DomainBlock.DecompressionShift)
                    G = (int)(0.75 * (DomainPixels[i * 2, j * 2][1]) + DomainBlock.DecompressionShift)
                    B = (int)(0.75 * (DomainPixels[i * 2, j * 2][2]) + DomainBlock.DecompressionShift)
                    draw.point((i, j), (R, G, B))
                    #Создаём новые изображения для каждого рангового блока
            FinalRangeBlock = BlockClass.Block(Bufferimage, range_size, 0, 0, 0)
            #FinalRangeBlock.Show()
            FinalRangeBlock.SetCoordinate(RangeBlock.coor_x, RangeBlock.coor_y)
            FinalRangeBlockList.append(FinalRangeBlock)
        FinalRangeBlockList.reverse()

        Newdraw = ImageDraw.Draw(newimage)  # Создаем инструмент для рисования.
        FinalRangeBlockListCopy = FinalRangeBlockList.copy()
        while(FinalRangeBlockListCopy):
            FinalRangeBlock = FinalRangeBlockListCopy.pop()
            #FinalRangeBlock.Show()
            FinalRangePixels = FinalRangeBlock.Blockimage.load()
            for i in range(range_size):
                for j in range(range_size):
                    R = FinalRangePixels[i, j][0]
                    G = FinalRangePixels[i, j][1]
                    B = FinalRangePixels[i, j][2]
                    Newdraw.point(((i + FinalRangeBlock.coor_x * range_size), (j + FinalRangeBlock.coor_y * range_size)),(R, G, B))

        newimage.save("Expanded file.png")
        newimage.close()
        decFile.close()




