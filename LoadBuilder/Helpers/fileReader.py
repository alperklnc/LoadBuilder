import pandas as pd


def get_containers():
    data_containers = pd.read_excel('/Users/alperkilinc/Desktop/KU/INDR491/LoadBuilder/LoadBuilder/container_dimensions.xlsx')
    df_containers = pd.DataFrame(data_containers)
    containers = df_containers.set_index("Container").T.to_dict()
    return containers


def get_materials():
    data_objects = pd.read_excel('/Users/alperkilinc/Desktop/KU/INDR491/LoadBuilder/LoadBuilder/material_dimensions.xlsx', skiprows=range(1, 3))
    df_objects = pd.DataFrame(data_objects, columns=["Material", "Product Hierarchy 2", "Length", "Width", "Height"])
    df_objects.rename(columns={"Product Hierarchy 2": "type", "Length": "length", "Width": "width", "Height": "height"}, inplace=True)
    materials = df_objects.set_index("Material").T.to_dict()
    return materials
