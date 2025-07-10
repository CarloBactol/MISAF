//let sidenav = document.querySelector(".sidenav");

let sidenavID = document.getElementById("mySidebar");
let div_mainID = document.getElementById("div_main");

/* function declare for side bar open and close */
function checkNav() {

    document.getElementById("mySidebar").classList.toggle("active");
    document.getElementById("div_main").classList.toggle("active");
}

function chooseHome() {
    document.getElementById("home").classList.toggle("active");
    document.getElementById("product").classList.remove("active");
    document.getElementById("about").classList.remove("active");
    document.getElementById("contact").classList.remove("active");
}

function chooseProduct() {
    document.getElementById("home").classList.remove("active");
    document.getElementById("product").classList.toggle("active");
    document.getElementById("about").classList.remove("active");
    document.getElementById("contact").classList.remove("active");
}

function chooseAbout() {
    document.getElementById("home").classList.remove("active");
    document.getElementById("product").classList.remove("active");
    document.getElementById("about").classList.toggle("active");
    document.getElementById("contact").classList.remove("active");
}

function chooseContact() {
    document.getElementById("home").classList.remove("active");
    document.getElementById("product").classList.remove("active");
    document.getElementById("about").classList.remove("active");
    document.getElementById("contact").classList.toggle("active");
}


//function closeProdListModal() {
//    //document.getElementsByClassName("body").style.display
//    //document.getElementById("ProdListModal").classList.remove("modal-backdrop");
//    ////document.getElementById("ProdListModal").classList.remove("fade");
//    ////document.getElementById("ProdListModal").classList.remove("show");
//    ////"$('body').removeClass('modal-open');$('.modal-backdrop').remove();$('#Div3').hide();"
//    //"$('body').classList.remove("modal-open");
//    //let modalbackdrop = document.getElementByClassName("modal-backdrop");
//    //modal-backdrop.parentNode.removeChild(modal-backdrop);
//    //document.getElementById("Div3").style.display = "none";

//    const div = document.querySelector('body') // Get element from DOM
//    div.classList.remove('modal-open') // Remove class "info"

//    div1 = document.getElementsByClassName("modal-backdrop");

//    while (div1.length) {
//        div1[0].classList.remove("modal-backdrop");
//    }
//    //document.getElementById("ProdListModal").classList.toggel("hide");
//    document.getElementById("ProdListModal").classList.toggle("hide");
//    document.getElementById("ProdListModal").style.display = "none";

//    document.getElementById("closeProdListModal").click();
//}


/* function use on tooltip */
//var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
//var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
//    return new bootstrap.Tooltip(tooltipTriggerEl)
//})



function listOnClick() {

    //var header = document.getElementById("mySidebar");
    //var btns = header.getElementsByClassName("list");
    //for (var i = 0; i < btns.length; i++) {
    //    alert('222312');
    //    btns[i].addEventListener("click", function () {
    //        var current = document.getElementsByClassName("active");
    //        current[0].className = current[0].className.replace(" active", "");
    //        this.className += " active";

    //    });
    //}

    //var current = document.getElementsByClassName("active");
    //current[0].className = current[0].className.replace(" active", "");
    //this.className += " active";

    // document.getElementById("sd").classList.remove("active");
    //document.getElementById("sd").className("list").remove("active");

    //alert("sdfd");
    //let list = document.querySelectorAll('.list');
    //let j = 0;
    //while (j < list.length) {
    //    list[j++].className = 'list';
    //    alert("asas");
    //}
    //list[i].className = 'list active';
    // document.getElementsByClassName(".list").classlist.toggle("active");


    //var header = document.getElementById("mySidebar");
    //var btns = header.getElementsByClassName("list");

    ////let list = document.querySelectorAll('.list');
    //for (let i = 0; i < btns.length; i++) {
    //   // alert("sdfd111");
    //    //btns[i].onclick = function () {
    //    //    let j = 0;
    //    //    while (j < btns.length) {
    //    //        btns[j++].className = 'list';
    //    //        alert("wawa");
    //    //    }
    //    //    btns[i].className = 'list';
    //    //}
    //    //btns[i].className = "list";
    //    //btns[i].className = btns[i].className.remove("list");
    //    btns[i].className = btns[i].className.replace(" active", " active");



    //            //var b = document.querySelector("ul li.active");
    //            //if (b) b.classList.remove("active");
    //            //this.parentNode.classList.add('active');

    //    alert("ttt");

}


//document.getElementById("sd").classList.remove("active");

// Add active class to the current button (highlight it)


//}
//alert("tttrtr");
//var header = document.getElementById("mySidebar");
//var zz = header.querySelectorAll("list");
//// var btns = header.getElementsByClassName("list");
//var a = header.querySelectorAll("list");
//for (let i = 0; i < a.length; i++) { {
//    a[i].onclick = function () {
//        alert("ttt");
//        var b = document.querySelector("ul li.active");
//        if (b) b.classList.remove("active");
//        this.parentNode.classList.add('active');
//    };
//}


function myFunctionB(x) {
    var mySidebar = !!document.getElementById("mySidebar");
    var div_main = !!document.getElementById("div_main");

    if (x.matches) { // If media query matches
        if (mySidebar) {
            document.getElementById("mySidebar").classList.remove("active");
            document.getElementById("mySidebar").classList.toggle("active");
        }

        if (div_main) {
            document.getElementById("div_main").classList.remove("active");
            document.getElementById("div_main").classList.toggle("active");
        }

    } else {
        if (mySidebar) { document.getElementById("mySidebar").classList.remove("active"); }
        if (div_main) { document.getElementById("div_main").classList.remove("active"); }
    }


    //- EDWIN LOPEZ - 06/07/2022
    //- NAGEERROR KASI KAYA NEED ICHECK IF NOT NULL.

    //if (x.matches) { // If media query matches
    //    document.getElementById("mySidebar").classList.remove("active");
    //    document.getElementById("div_main").classList.remove("active");
    //    document.getElementById("mySidebar").classList.toggle("active");
    //    document.getElementById("div_main").classList.toggle("active");
    //} else {
    //    document.getElementById("mySidebar").classList.remove("active");
    //    document.getElementById("div_main").classList.remove("active");
    //}
}

var x = window.matchMedia("(min-width: 600px)")
x.addListener(myFunctionB) // Attach listener function on state changes
myFunctionB(x) // Call listener function at run time

//"$('body').removeClass('modal-open');$('.modal-backdrop').remove();"
function aa() {
}


// Clicking dropdown button will toggle display
function btnToggle() {
    //alert("sese");
    //var myCollapse = document.getElementById('homeSubmenu')
    //var bsCollapse = new bootstrap.Collapse(myCollapse, {
    //    toggle: false
    //})
    document.getElementById("homeSubmenu").classList.remove("collapse");
    //document.getElementById("homeSubmenu").classList.toggle("show");
    document.getElementById("mainMasterPage").src = "/LeadManagement/Lead_Profile.aspx";
}


    //function openModal() {
    //    //$('#mod_confirmvalidate_crd').modal('show');
    //    var myModal1 = new bootstrap.Modal(document.getElementById('filterModal'));
    //    var myModal = new boostrap.Modal(document.body.appendChild(document.getElementById('filterModal')));
    //    myModal.show();
    //    var mm = new bootstrao.Modal();
    //    myModal1.appendChild(document.getElementsByClassName('body'));
    //}



    //function onClickC() {
    //    var number = Math.round(Math.random() * 100000);
    //    new row = '<tr data-bs-toggle="collapse" data-bs-target=".row' + number + '-child"> <td>This is a dynamically created parent row. Click me to toggle childs.<a class="child-link" href="!#">A Link!</a><button class="child-button">A button!</button></td></tr><tr class="collapse child row';
    //    +number + '-child"><td>Im a child row!</td></tr><tr class="collapse child row';
    //    +number + '-child"><td>Im another child row!</td></tr> <tr class="collapse child row';
    //    +number + '-child"><td>Im yet another child row!</td></tr>';
    //    $("#main-table").append(new_row);
    //}

    //$("#createNewRow").on("click", function() {
    //    var number = Math.round(Math.random() * 100000);
    //    new row = '<tr data-bs-toggle="collapse" data-bs-target=".row' + number + '-child"> <td>This is a dynamically created parent row. Click me to toggle childs.<a class="child-link" href="!#">A Link!</a><button class="child-button">A button!</button></td></tr><tr class="collapse child row' ;
    //    + number + '-child"><td>Im a child row!</td></tr><tr class="collapse child row';
    //    + number + '-child"><td>Im another child row!</td></tr> <tr class="collapse child row' ;
    //    + number + '-child"><td>Im yet another child row!</td></tr>';
    //    $("#main-table").append(new_row);
    //});


    //    var eventHandler = function (e) {
    //        console.log("The collapse event was prevented!", e);
    //        e.preventDefault();
    //        e.stopPropagation();
    //    }

    //    $(".row1-child").on("show.bs.collapse", eventHandler)

    //    $(".child-link").on("click", function(e) {
    //        console.log("The link was clicked!");
    //        e.stopPropagation();
    //        e.preventDefault();
    //    });

    //    $(".child-button").on("click", function(e) {
    //        console.log("The button was clicked!");
    //        e.stopPropagation();
    //        e.preventDefault();
    //    });

    //    $("[data-bs-toggle]").on("click", function(e) {    
    //        console.log("The parent was clicked! dispatch");
    //        $(".row1-child").off("show.bs.collapse", eventHandler)
    //        $(".row1-child").off("hide.bs.collapse", eventHandler)
    //        $(".row1-child").collapse('toggle')
    //        $(".row1-child").on("show.bs.collapse", eventHandler)
    //        $(".row1-child").on("hide.bs.collapse", eventHandler)
    //    });


//const fileSelect = document.getElementById("fileSelect"),
//               fileElem = document.getElementById("fileElem"),
//               fileList = document.getElementById("fileList");

//fileSelect.addEventListener("click", function (e) {
//    if (fileElem) {
//        fileElem.click();
//    }
//    e.preventDefault(); // prevent navigation to "#"
//}, false);

//fileElem.addEventListener("change", handleFiles, false);

//function handleFiles() {
//    if (!this.files.length) {
//        fileList.innerHTML = "<p>No files selected!</p>";
//    } else {
//        fileList.innerHTML = "";
//        const list = document.createElement("ul");
//        fileList.appendChild(list);
//        for (let i = 0; i < this.files.length; i++) {
//            const li = document.createElement("li");
//            list.appendChild(li);

//            const img = document.createElement("img");
//            img.src = URL.createObjectURL(this.files[i]);
//            img.height = 60;
//            img.onload = function () {
//                URL.revokeObjectURL(this.src);
//            }
//            li.appendChild(img);
//            const info = document.createElement("span");
//            info.innerHTML = this.files[i].name + ": " + this.files[i].size + " bytes";
//            li.appendChild(info);
//        }
//    }
//}

