# city-from-gmlfile
## Spécifications
    Project Version : Unity 2020.3.23f1
## Données nécessaires
- Télécharger le dossier CityGML de Bron : https://download.data.grandlyon.com/files/grandlyon/imagerie/2018/maquette/BRON_2018.zip
- Copier le fichier "BRON_TIN_2018.gml" dans **Assets/gml** (le fichier, pas le dossier !)

# GeoRoads

Par Delaunay Evann & Feuillastre Donnovan

[Rapport](https://docs.google.com/document/d/1FmAm7-7gqJv9aSPK4r23yALgCN8vLrTkClIRi87-S0E/edit?usp=sharing), [Présentation](https://docs.google.com/presentation/d/10v7n7tFI3dTT6-j4kwbwoI0YFQ5D2lqAXltg3CUg26s/edit?usp=sharing)

## Récupération de données

Nous avons dû récupérer des données GeoGML pour créer la route, étant donné que Unity ne connaît pas ce format de fichier, nous devions trouver une solution. 
Soit écrire un parseur de ce genre de données, soit trouver une librairie existante. 
Nous avons opté pour la seconde solution et modifié un peu le parseur pour permettre certains types non pris en compte ([lien parseur](https://github.com/timokorkalainen/Unity-GeoJSONObject)).

## Conversion des données

Les données étant au format GPS, nous avons dû les convertir. 
Cependant, Unity gère les nombres flottant en 32bits et les données passées sont bien plus élevées. 
Il y aura donc une perte de données obligatoire ([lien explication](https://blog.mapbox.com/wgs84-precision-in-unity-world-space-687c7d574bb3)). 
Tout comme pour la lecture des données, une librairie l'avait déjà fait dans les issues du parseur de GeoGML ([lien convertisseur](https://github.com/MichaelTaylor3D/UnityGPSConverter), [GPStoUCS](Assets/Scripts/GPSEncoder/GPSEncoder.cs#L114)).

## Création des routes

Avec des données extraites on génère les différentes routes dans unity, pour cela on génère un modèle 3D par route à l’aide d’une liste de points, chaque point contient un vecteur 2D et une hauteur afin de rendre plus simple les calculs.
Le mesh de la route est fait de segments perpendiculaires à la route connectés par deux triangles, la première étape est de trouver leur direction et longueur, dans le cas des points de départ et de fin c’est simple on calcul la normale de la route et on prend sa largeur de base. Cependant la situation se complique dans les coins car le segment doit être plus long sinon la route sera comme pincée.

<p align="center">
    <img src="Images/parralelle_droite.png">
    </img>
</p>

A gauche tous les segments ont la même longueur, à droite le segment du centre est rallongé pour éviter le pincement. Pour cela on utilise une méthode simple, on commence par calculer la tangente du coin:

<p align="center">
    <b>
        (Direction sortie normalisé + Direction entrée normalisé) le tout normalisé
    </b>
</p>

Ensuite on prend la normale de la tangente (miter), comme on travaille en 2D il suffit de faire 

<p align="center">
    <b>
        [-tangente.x , tangente.y]
    </b>
</p>

Et enfin pour sa longueur on divise la largeur de base de la route par le produit scalaire du miter et de la normale de la direction d’entrée.
Pour rajouter un peu plus de qualité on a aussi arrondis le coin extérieur de la route, pour cela il suffit de prendre un cercle au centre du segment avec comme diamètre la largeur de la route et de générer plusieurs points sur l’arc du cercle présent entre la normale du segment d’entrée et de sortie.

<p align="center">
    <img src="Images/texture_flou.png">
    </img>
</p>
