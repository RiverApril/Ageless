from PIL import Image
import os


path = os.path.dirname(os.path.realpath(__file__))


def crop(path, name, nameformat, chunkX, chunkY, chunkW, chunkH, overlap):
    im = Image.open(os.path.join(path, name))
    imgW, imgH = im.size
    for i in range(0, imgH - overlap, chunkH):
        for j in range(0, imgW - overlap, chunkW):
            box = (j, i, j + chunkW + overlap, i + chunkH + overlap)
            a = im.crop(box)
            try:
                a.save(os.path.join(path, nameformat.format((j / chunkW) + chunkX, (i / chunkH) + chunkY)))
            except:
                print "Failed to save"


crop(path, "full.png", "htmp.{}.{}.f.s.a.png", -2, -2, 128, 128, 1)
