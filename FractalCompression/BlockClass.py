from PIL import Image, ImageDraw

#Класс описывающий блоки, на которые разбивается исходное изображение
class Block:

    # Инициализация
    def __init__(self, image, size, i, j, rotate):
        # Начальные координаты
        self.coor_x = j * size
        self.coor_y = i * size
        # Коэффицент преобразования
        self.coeff = rotate
        self.DecompressionShift = 0
        area = (j * size, i * size, (j + 1) * size, (i + 1) * size)

        self.Blockimage = image.crop(area)
        # Повороты изображения
        if (rotate == 0):
            self.Blockimage = self.Blockimage.rotate(0)
        if (rotate == 1):
            self.Blockimage = self.Blockimage.rotate(90)
        if (rotate == 2):
            self.Blockimage = self.Blockimage.rotate(180)
        if (rotate == 3):
            self.Blockimage = self.Blockimage.rotate(270)
        # Зеркальное отражение и так же повороты изображения
        if (rotate == 4):
            self.Blockimage = self.Blockimage.transpose(1)
            self.Blockimage = self.Blockimage.rotate(0)
        if (rotate == 5):
            self.Blockimage = self.Blockimage.transpose(1)
            self.Blockimage = self.Blockimage.rotate(90)
        if (rotate == 6):
            self.Blockimage = self.Blockimage.transpose(1)
            self.Blockimage = self.Blockimage.rotate(180)
        if (rotate == 7):
            self.Blockimage = self.Blockimage.transpose(1)
            self.Blockimage = self.Blockimage.rotate(270)

    # Вывод изображения
    def Show(self):
        self.Blockimage.show()

    # Функция для задания сдвига по яркости
    def SetShift(self, shift):
        self.DecompressionShift = shift

    # Функция для задания начальных координат блока вне конструктора
    def SerCoordinate(self, coor_x, coor_y):
        self.coor_x = coor_x
        self.coor_y = coor_y