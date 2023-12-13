//Gets a regular hexagon defined by the circle defined by point p and measure m.
//Gets the mediatrix of the line defined by p1 and p2.
mediatrix(p1, p2) = 
    let
        l1 = line(p1, p2);
        draw l1;
        m = measure (p1, p2);
        c1 = circle (p1, m);
        c2 = circle (p2, m);
        draw {c1, c2};
        i1,i2,_ = intersect(c1, c2);
        draw line(i1, i2);
    in line(i1,i2);

point p1;
point p2;
draw mediatrix(p1, p2);
