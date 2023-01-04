import sys
import pandas as pd
import decimal
from decimal import Decimal


DEFAULT_NUMBER_OF_DECIMALS = 3
START_POSITION = [0, 0, 0]

def rect_intersect(item1, item2, x, y):
    d1 = item1.get_dimension()
    d2 = item2.get_dimension()

    cx1 = item1.position[x] + d1[x]/2
    cy1 = item1.position[y] + d1[y]/2
    cx2 = item2.position[x] + d2[x]/2
    cy2 = item2.position[y] + d2[y]/2

    ix = max(cx1, cx2) - min(cx1, cx2)
    iy = max(cy1, cy2) - min(cy1, cy2)

    return ix < (d1[x]+d2[x])/2 and iy < (d1[y]+d2[y])/2


def intersect(item1, item2):
    return (
        rect_intersect(item1, item2, Axis.WIDTH, Axis.HEIGHT) and
        rect_intersect(item1, item2, Axis.HEIGHT, Axis.DEPTH) and
        rect_intersect(item1, item2, Axis.WIDTH, Axis.DEPTH)
    )


def get_limit_number_of_decimals(number_of_decimals):
    return Decimal('1.{}'.format('0' * number_of_decimals))


def set_to_decimal(value, number_of_decimals):
    number_of_decimals = get_limit_number_of_decimals(number_of_decimals)

    return Decimal(value).quantize(number_of_decimals)

class RotationType:
    RT_WHD = 0
    RT_HWD = 1
    RT_HDW = 2
    RT_DHW = 3
    RT_DWH = 4
    RT_WDH = 5
    
    ALL = [RT_WHD, RT_HWD, RT_HDW, RT_DHW, RT_DWH, RT_WDH]


class Axis:
    WIDTH = 0
    HEIGHT = 1
    DEPTH = 2

    ALL = [WIDTH, HEIGHT, DEPTH]

    
rotation_type = RotationType.RT_HDW

class Item:
    def __init__(self, name, width, height, depth, weight, loading_type, item_id):
        self.name = name
        self.width = width
        self.height = height
        self.depth = depth
        self.weight = weight
        self.loading_type = loading_type
        self.rotation_type = 0
        self.position = START_POSITION
        self.number_of_decimals = DEFAULT_NUMBER_OF_DECIMALS
        self.item_id = item_id

    def format_numbers(self, number_of_decimals):
        self.width = set_to_decimal(self.width, number_of_decimals)
        self.height = set_to_decimal(self.height, number_of_decimals)
        self.depth = set_to_decimal(self.depth, number_of_decimals)
        self.weight = set_to_decimal(self.weight, number_of_decimals)
        self.number_of_decimals = number_of_decimals

    def string(self):
        return "%s(%sx%sx%s, weight: %s) pos(%s) rt(%s) vol(%s)" % (
            self.name, self.width, self.height, self.depth, self.weight,
            self.position, self.rotation_type, self.get_volume()
        )

    def get_volume(self):
        return set_to_decimal(
            self.width * self.height * self.depth, self.number_of_decimals
        )

    def get_depth_item(self):
        return self.depth
    
    def get_width_item(self):
        return self.width
    
    def get_height_item(self):
        return self.height
    
    def get_item_id(self):
        return self.item_id
    
    def get_position(self):
        return self.position
    
    def get_dimension(self):
        if self.rotation_type == RotationType.RT_WHD:
            dimension = [self.width, self.height, self.depth]
        elif self.rotation_type == RotationType.RT_HWD:
            dimension = [self.height, self.width, self.depth]
        elif self.rotation_type == RotationType.RT_HDW:
            dimension = [self.height, self.depth, self.width]
        elif self.rotation_type == RotationType.RT_DHW:
            dimension = [self.depth, self.height, self.width]
        elif self.rotation_type == RotationType.RT_DWH:
            dimension = [self.depth, self.width, self.height]
        elif self.rotation_type == RotationType.RT_WDH:
            dimension = [self.width, self.depth, self.height]
        else:
            dimension = []

        return dimension


class Bin:
    def __init__(self, name, width, height, depth, max_weight):
        self.name = name
        self.width = width
        self.height = height
        self.depth = depth
        self.max_weight = max_weight
        self.items = []
        self.unfitted_items = []
        self.number_of_decimals = DEFAULT_NUMBER_OF_DECIMALS
    

    def format_numbers(self, number_of_decimals):
        self.width = set_to_decimal(self.width, number_of_decimals)
        self.height = set_to_decimal(self.height, number_of_decimals)
        self.depth = set_to_decimal(self.depth, number_of_decimals)
        self.max_weight = set_to_decimal(self.max_weight, number_of_decimals)
        self.number_of_decimals = number_of_decimals

    def string(self):
        return "%s(%sx%sx%s, max_weight:%s) vol(%s)" % (
            self.name, self.width, self.height, self.depth, self.max_weight,
            self.get_volume()
        )

    def get_volume(self):
        return set_to_decimal(
            self.width * self.height * self.depth, self.number_of_decimals
        )
    
    def get_name(self):
        return self.name
    
    def get_depth_bin(self):
        return self.depth
    
    def get_width_bin(self):
        return self.width
    
    def get_height_bin(self):
        return self.height

    def get_total_weight(self):
        total_weight = 0

        for item in self.items:
            total_weight += item.weight

        return set_to_decimal(total_weight, self.number_of_decimals)

    def put_item(self, item, pivot):
        fit = False
        valid_item_position = item.position
        item.position = pivot
        
        rotation_types = range(0, len(RotationType.ALL))
        
        if item.loading_type == "OnlyVertical":
            rotation_types = [0, 1]

        for i in rotation_types:
            item.rotation_type = i
            dimension = item.get_dimension()
            if (
                self.width < pivot[0] + dimension[0] or
                self.height < pivot[1] + dimension[1] or
                self.depth < pivot[2] + dimension[2]
            ):
                continue

            fit = True

            for current_item_in_bin in self.items:
                if intersect(current_item_in_bin, item):
                    fit = False
                    break

            if fit:

                if self.get_total_weight() + item.weight > self.max_weight:
                    fit = False
                    return fit

                self.items.append(item)

            if not fit:
                item.position = valid_item_position

            return fit

        if not fit:
            item.position = valid_item_position

        return fit


class Packer:
    def __init__(self):
        self.bins = []
        self.items = []
        self.placed_items = []
        self.unfit_items = []
        self.total_items = 0
        self.utilization= 0.0
        
    def get_utilization(self):
        return self.utilization 
    
    def get_placed_items(self):
        return self.placed_items

    def add_bin(self, bin):
        return self.bins.append(bin)

    def add_item(self, item):
        self.total_items = len(self.items) + 1

        return self.items.append(item)

    def pack_to_bin(self, bin, item):
        fitted = False

        if not bin.items:
            response = bin.put_item(item, START_POSITION)

            if not response:
                bin.unfitted_items.append(item)

            return

        for axis in range(0, 3):
            items_in_bin = bin.items

            for ib in items_in_bin:
                pivot = [0, 0, 0]
                w, h, d = ib.get_dimension()
                if axis == Axis.WIDTH:
                    pivot = [
                        ib.position[0] + w,
                        ib.position[1],
                        ib.position[2]
                    ]
                elif axis == Axis.HEIGHT:
                    pivot = [
                        ib.position[0],
                        ib.position[1] + h,
                        ib.position[2]
                    ]
                elif axis == Axis.DEPTH:
                    pivot = [
                        ib.position[0],
                        ib.position[1],
                        ib.position[2] + d
                    ]

                if bin.put_item(item, pivot):
                    fitted = True
                    break
            if fitted:
                break

        if not fitted:
            bin.unfitted_items.append(item)

    def pack(
        self, bigger_first=False, distribute_items=False,
        number_of_decimals=DEFAULT_NUMBER_OF_DECIMALS
    ):
        for bin in self.bins:
            bin.format_numbers(number_of_decimals)

        for item in self.items:
            item.format_numbers(number_of_decimals)

        self.bins.sort(
            key=lambda bin: bin.get_volume(), reverse=bigger_first
        )
        self.items.sort(
            key=lambda item: item.get_volume(), reverse=bigger_first
        )

        for bin in self.bins:
            for item in self.items:
                self.pack_to_bin(bin, item)
                

            if distribute_items:
                for item in bin.items:
                    self.items.remove(item)

class Order_Info:
    def __init__(self, order_id, country_name, item_types):
        self.order_id = order_id
        self.country_name = country_name
        self.item_types =  item_types
     
    def get_order_id(self):
        return self.order_id
    
    def get_country_name(self):
        return self.country_name



class Container_Info:
    def __init__(self,container_name, x,y,z):
        self.container_name= container_name
        self.x = x
        self.y = y
        self.z = z

    def get_container_name(self):
        return self.container_name
    
    def get_x(self):
        return self.x
    
    def get_y(self):
        return self.y
    
    def get_z(self):
        return self.z



class Box_Info:
    def __init__(self, i_d, amount, x, y, z, rotation_type):
        self.i_d = i_d
        self.amount = amount
        self.x = x
        self.y = y
        self.z = z
        self.rotation_type = rotation_type
       
    def get_amount(self):
        return self.amount
    
    def get_i_d(self):
        return self.i_d
    
    def get_x(self):
        return self.x
    
    def get_y(self):
        return self.y
    
    def get_z(self):
        return self.z
    
    def get_rotation_type(self):
        return self.rotation_type


# args from system call
path = sys.argv[1]
file_name = sys.argv[2]

order = []
containers = []
boxes_ordered = []

with open(path + "/" + file_name, 'r') as f:
    lines = f.readlines()
    
f.close()

for line in lines:
    values = line.split()
    if len(values) == 3:
        # This is the first line with order_id, country_name, and pisink
        order_id = values[0]
        country_name = values[1]
        item_types = values[2]
        order.append(Order_Info(order_id, country_name, item_types))
    
    elif len(values) == 4:
        # This is the second line with container name and dimensions
        container_name = values[0]
        x = values[1]
        y = values[2]
        z = values[3]
        containers.append(Container_Info(container_name, x, y, z))
    
    else:
        # This is the third line with box id, amount, dimensions, and rotation type
        i_d = values[0]
        amount = values[1]
        x = values[2]
        y = values[3]
        z = values[4]
        rotation_type = values[5]
        boxes_ordered.append(Box_Info(i_d, amount, x, y, z, rotation_type))


packer = Packer()

container_1 = Bin(containers[0].get_container_name(),containers[0].get_x(),containers[0].get_y(),containers[0].get_z(),9999.0)
packer.add_bin(container_1)

for item in boxes_ordered:
    for i in range(int(item.get_amount())):
        packer.add_item(Item('item',item.get_x(),item.get_y(), item.get_z(), 1.0 , item.get_rotation_type(),item.get_i_d()))
        
packer.pack()           
    
with open(path + '/bestfit_output.txt', 'w') as file:
    file.write(f"{order[0].get_order_id()} {order[0].get_country_name()} {container_1.get_name()} {packer.get_utilization()}")
    file.write(f"\n{container_1.get_depth_bin()} {container_1.get_width_bin()} {container_1.get_height_bin()}")

    for item in packer.bins[0].items:
        file.write(f"\n{item.get_position()[0]} {item.get_position()[1]} {item.get_position()[2]} {item.get_depth_item()} {item.get_width_item()} {item.get_height_item()} {item.get_item_id()}  ")

file.close()