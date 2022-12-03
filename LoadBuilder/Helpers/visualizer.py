import sys
from py3dbp import Packer, Bin, Item
from mpl_toolkits.mplot3d import Axes3D
from mpl_toolkits.mplot3d.art3d import Poly3DCollection
import numpy as np
import matplotlib.pyplot as plt
import random

path = sys.argv[1]
file_name = sys.argv[2]

positions = []
sizes = []
colors = []

trucks = [
    [1204, 235, 269]
]
truckX = trucks[0][0]
truckY = trucks[0][1]
truckZ = trucks[0][2]

file_path = path + "/" + file_name + ".txt"
with open(file_path) as f:
    for line in f:
        box_info = line.split()

        position = (float(box_info[0]), float(box_info[1]), float(box_info[2]))
        positions.append(position)

        size = (float(box_info[3]), float(box_info[4]), float(box_info[5]))
        sizes.append(size)


def cuboid_data2(o, size=(1, 1, 1)):
    X = [[[0, 1, 0], [0, 0, 0], [1, 0, 0], [1, 1, 0]],
         [[0, 0, 0], [0, 0, 1], [1, 0, 1], [1, 0, 0]],
         [[1, 0, 1], [1, 0, 0], [1, 1, 0], [1, 1, 1]],
         [[0, 0, 1], [0, 0, 0], [0, 1, 0], [0, 1, 1]],
         [[0, 1, 0], [0, 1, 1], [1, 1, 1], [1, 1, 0]],
         [[0, 1, 1], [0, 0, 1], [1, 0, 1], [1, 1, 1]]]
    X = np.array(X).astype(float)
    for i in range(3):
        X[:, :, i] *= size[i]
    X += np.array(o)
    return X


def plotCubeAt2(positions, sizes=None, colors=None, **kwargs):
    if not isinstance(colors, (list, np.ndarray)): colors = ["C0"] * len(positions)
    if not isinstance(sizes, (list, np.ndarray)): sizes = [(1, 1, 1)] * len(positions)
    g = []
    for p, s, c in zip(positions, sizes, colors):
        g.append(cuboid_data2(p, size=s))
    return Poly3DCollection(np.concatenate(g), facecolors=np.repeat(colors, 6), **kwargs)


colorList = ["crimson", "limegreen", "g", "r", "c", "m", "y", "k"]

for i in range(len(positions)):
    f = random.randint(0, 7)
    colors.append(colorList[f])

fig = plt.figure(figsize=plt.figaspect(1) * 3)
ax = fig.add_subplot(projection='3d')

pc = plotCubeAt2(positions, sizes, colors, edgecolor="k")
ax.add_collection3d(pc)

ax.set_xlim3d([0, truckX])
ax.set_ylim3d([0, truckY])
ax.set_zlim3d([0, truckZ])

"""                                                                                                                                                    
Scaling is done from here...                                                                                                                           
"""
x_scale = truckX
y_scale = truckY
z_scale = truckZ

scale = np.diag([x_scale, y_scale, z_scale, 1.0])
scale = scale * (1.0 / scale.max())
scale[3, 3] = 1.0


def short_proj():
    return np.dot(Axes3D.get_proj(ax), scale)


ax.get_proj = short_proj
"""                                                                                                                                                    
to here                                                                                                                                                
"""

png_path = path + "/" + file_name + ".png"
plt.savefig(png_path, bbox_inches='tight')
plt.show()
