//$(document).ready(function () {
//    "use strict";
//    $("#order_list-datatable").DataTable({
//        language: {
//            paginate: {
//                previous: "<i class='mdi mdi-chevron-left'>",
//                next: "<i class='mdi mdi-chevron-right'>"
//            },
//            info: "Showing Orders _START_ to _END_ of _TOTAL_",
//            lengthMenu: 'Display <select class=\'form-select form-select-sm ms-1 me-1\'><option value="5">5</option><option value="10">10</option><option value="20">20</option><option value="-1">All</option></select> Orders'
//        },
//        pageLength: 5,
//        select: { style: "multi" },
//        order: [[2, "desc"], [0, "desc"]],
//        drawCallback: function () {
//            $(".dataTables_paginate > .pagination").addClass("pagination-rounded"), $("#order_list-datatable_length label").addClass("form-label"), document.querySelector(".dataTables_wrapper .row").querySelectorAll(".col-md-6").forEach(function (e) {
//                e.classList.add("col-sm-6"), e.classList.remove("col-sm-12"), e.classList.remove("col-md-6")
//            })
//        }
//    })
//});